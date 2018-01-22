using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.CoinInfoService.Storage;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Data;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.Infrastructure;
using Msv.AutoMiner.Common.Log;
using Msv.AutoMiner.Common.Models.CoinInfoService;
using Msv.AutoMiner.Common.ServiceContracts;
using Msv.AutoMiner.Data.Logic;

namespace Msv.AutoMiner.CoinInfoService.Controllers
{
    [Route("api/[controller]")]
    public class CoinInfoController : Controller, ICoinInfoService
    {
        private const int CryptoCurrencyDecimalPlaces = 8;
        private const int FiatDecimalPlaces = 4;

        private readonly IStoredFiatValueProvider m_FiatProvider;
        private readonly IProfitabilityCalculator m_Calculator;
        private readonly ICoinNetworkInfoProvider m_CoinNetworkInfoProvider;
        private readonly ICoinValueProvider m_CoinValueProvider;
        private readonly ICoinInfoControllerStorage m_Storage;

        public CoinInfoController(IStoredFiatValueProvider fiatProvider, IProfitabilityCalculator calculator,
            ICoinNetworkInfoProvider coinNetworkInfoProvider, ICoinValueProvider coinValueProvider,
            ICoinInfoControllerStorage storage)
        {
            m_FiatProvider = fiatProvider ?? throw new ArgumentNullException(nameof(fiatProvider));
            m_Calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
            m_CoinNetworkInfoProvider = coinNetworkInfoProvider ?? throw new ArgumentNullException(nameof(coinNetworkInfoProvider));
            m_CoinValueProvider = coinValueProvider ?? throw new ArgumentNullException(nameof(coinValueProvider));
            m_Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        [HttpGet("getAlgorithms")]
        //[ValidateApiKey(ApiKeyType.CoinInfoService, false)]
        public Task<AlgorithmInfo[]> GetAlgorithms() 
            => Task.FromResult(m_Storage.GetAlgorithms()
                .Select(x => new AlgorithmInfo
                {
                    Id = x.Id,
                    KnownValue = x.KnownValue,
                    Name = x.Name
                })
                .ToArray());

        [HttpPost("getProfitabilities")]
        //[ValidateApiKey(ApiKeyType.CoinInfoService)]
        public Task<ProfitabilityResponseModel> GetProfitabilities([FromBody] ProfitabilityRequestModel request)
        {
            var networkInfos = request.DifficultyAggregationType == ValueAggregationType.Last
                ? m_CoinNetworkInfoProvider.GetCurrentNetworkInfos(true)
                : m_CoinNetworkInfoProvider.GetAggregatedNetworkInfos(true,
                    GetMinDateTime(request.DifficultyAggregationType));

            var marketPrices = request.PriceAggregationType == ValueAggregationType.Last
                ? m_CoinValueProvider.GetCurrentCoinValues(true)
                : m_CoinValueProvider.GetAggregatedCoinValues(true, GetMinDateTime(request.PriceAggregationType));

            var btcUsdValue = m_FiatProvider.GetLastBtcUsdValue();
            var profitabilities = request.AlgorithmDatas
                .Join(networkInfos, x => x.AlgorithmId, x => x.Coin.AlgorithmId,
                    (x, y) => (networkInfo: y, algorithmInfo: x))
                .Join(marketPrices, x => x.networkInfo.CoinId, x => x.CurrencyId,
                    (x, y) => new
                    {
                        NetworkInfo = x.networkInfo,
                        AlgorithmInfo = x.algorithmInfo,
                        MarketPrices = y.ExchangePrices.EmptyIfNull(),
                        CoinsPerDay = Math.Round(m_Calculator.CalculateCoinsPerDay(
                                x.networkInfo.Difficulty, x.networkInfo.BlockReward,
                                x.networkInfo.Coin.MaxTarget, x.algorithmInfo.NetHashRate),
                            CryptoCurrencyDecimalPlaces)
                    })
                .Select(x => new SingleProfitabilityData
                {
                    AlgorithmId = x.AlgorithmInfo.AlgorithmId,
                    Height = x.NetworkInfo.Height,
                    Difficulty = x.NetworkInfo.Difficulty,
                    BlockReward = x.NetworkInfo.BlockReward,
                    NetHashRate = x.NetworkInfo.NetHashRate,
                    CoinSymbol = x.NetworkInfo.Coin.Symbol,
                    CoinName = x.NetworkInfo.Coin.Name,
                    CoinId = x.NetworkInfo.CoinId,
                    CoinsPerDay = x.CoinsPerDay,
                    LastUpdatedUtc = x.NetworkInfo.Created,
                    ElectricityCostPerDay = GetElectricityCostPerDay(x.AlgorithmInfo.Power, request.ElectricityCostUsd),
                    MarketPrices = x.MarketPrices
                        .Where(y => y.IsActive)
                        .Select(y => new MarketPriceData
                        {
                            Exchange = y.Exchange,
                            BtcPerDay = Math.Round(x.CoinsPerDay * y.Price, CryptoCurrencyDecimalPlaces),
                            UsdPerDay = Math.Round(x.CoinsPerDay * y.Price * btcUsdValue.Value, FiatDecimalPlaces)
                        })
                        .ToArray()
                })
                .OrderBy(x => x.CoinName)
                .ToArray();
            return Task.FromResult(new ProfitabilityResponseModel
            {
                Profitabilities = profitabilities
            });
        }

        [HttpPost("estimateProfitability")]
        //[ValidateApiKey(ApiKeyType.CoinInfoService)]
        public Task<EstimateProfitabilityResponseModel> EstimateProfitability(
            [FromBody] EstimateProfitabilityRequestModel request)
        {
            var btcUsdValue = m_FiatProvider.GetLastBtcUsdValue();
            var coinsPerDay = m_Calculator.CalculateCoinsPerDay(
                request.Difficulty,
                request.BlockReward,
                request.MaxTarget,
                request.ClientHashRate);
            var btcPerDay = coinsPerDay * request.BtcPrice;
            var electricityCostPerDay = GetElectricityCostPerDay(request.ClientPowerUsage, request.ElectricityCostUsd);
            var result = new EstimateProfitabilityResponseModel
            {
                Coins = new EstimateProfitabilityResponseModel.CumulativeProfitability(coinsPerDay),
                Btc = new EstimateProfitabilityResponseModel.CumulativeProfitability(btcPerDay),
                Usd = new EstimateProfitabilityResponseModel.CumulativeProfitability(
                    btcPerDay * btcUsdValue.Value - electricityCostPerDay)
            };
            return Task.FromResult(result);
        }

        [HttpGet("getLog")]
        //[ValidateApiKey(ApiKeyType.CoinInfoService)] // ONLY FOR INTERNAL SERVICE!!!!!!!!
        public Task<ServiceLogs> GetLog()
            => Task.FromResult(new ServiceLogs
            {
                Errors = MemoryBufferTarget.GetBuffer("ErrorLogBuffer"),
                Full = MemoryBufferTarget.GetBuffer("FullLogBuffer")
            });

        private static double GetElectricityCostPerDay(double unitCost, double powerUsageWatts)
            => Math.Round(powerUsageWatts / 1000 * unitCost * 24, FiatDecimalPlaces);

        private static DateTime GetMinDateTime(ValueAggregationType aggregationType)
        {
            var now = DateTime.UtcNow;
            switch (aggregationType)
            {
                case ValueAggregationType.Last12Hours:
                    return now.AddHours(-12);
                case ValueAggregationType.Last24Hours:
                    return now.AddDays(-1);
                case ValueAggregationType.Last3Days:
                    return now.AddDays(-3);
                case ValueAggregationType.LastWeek:
                    return now.AddDays(-7);
                case ValueAggregationType.Last2Weeks:
                    return now.AddDays(-14);
                default:
                    throw new ArgumentOutOfRangeException(nameof(aggregationType));
            }
        }
    }
}