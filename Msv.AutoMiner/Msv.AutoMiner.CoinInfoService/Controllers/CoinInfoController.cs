using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.CoinInfoService.Logic.Profitability;
using Msv.AutoMiner.CoinInfoService.Storage;
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
        private readonly ICoinInfoControllerStorage m_Storage;

        public CoinInfoController(IStoredFiatValueProvider fiatProvider, IProfitabilityCalculator calculator, ICoinInfoControllerStorage storage)
        {
            m_FiatProvider = fiatProvider ?? throw new ArgumentNullException(nameof(fiatProvider));
            m_Calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
            m_Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        [HttpGet("getAlgorithms")]
        //[ValidateApiKey(ApiKeyType.CoinInfoService, false)]
        public async Task<AlgorithmInfo[]> GetAlgorithms()
        {
            return (await m_Storage.GetAlgorithms())
                .Select(x => new AlgorithmInfo
                {
                    Id = x.Id,
                    KnownValue = x.KnownValue,
                    Name = x.Name
                })
                .ToArray();
        }

        [HttpPost("getProfitabilities")]
        //[ValidateApiKey(ApiKeyType.CoinInfoService)]
        public async Task<ProfitabilityResponseModel> GetProfitabilities([FromBody] ProfitabilityRequestModel request)
        {
            var networkInfos = await m_Storage.GetNetworkInfos(request.DifficultyAggregationType);
            var btc = await m_Storage.GetBtcCurrency();
            var marketPrices = (await m_Storage.GetExchangeMarketPrices(request.PriceAggregationType))
                .Where(x => x.TargetCoinId == btc.Id)
                .GroupBy(x => x.SourceCoinId)
                .ToDictionary(x => x.Key, x => x.ToArray());

            var btcUsdValue = await m_FiatProvider.GetLastBtcUsdValueAsync();
            var profitabilities = request.AlgorithmDatas
                .Join(networkInfos, x => x.AlgorithmId, x => x.Coin.AlgorithmId,
                    (x, y) => (networkInfo: y, algorithmInfo: x))
                .Join(marketPrices, x => x.networkInfo.CoinId, x => x.Key,
                    (x, y) => new
                    {
                        NetworkInfo = x.networkInfo,
                        AlgorithmInfo = x.algorithmInfo,
                        MarketPrices = y.Value,
                        CoinsPerDay =  Math.Round(m_Calculator.CalculateCoinsPerDay(
                            x.networkInfo.Coin, x.networkInfo, x.algorithmInfo.NetHashRate),
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
                    ElectricityCostPerDay = Math.Round(x.AlgorithmInfo.Power / 1000 * request.ElectricityCostUsd * 24, FiatDecimalPlaces),
                    MarketPrices = x.MarketPrices.Select(y => new MarketPriceData
                        {
                            Exchange = y.Exchange,
                            LastDayVolume = y.LastDayVolume,
                            BtcPerDay = Math.Round(x.CoinsPerDay * y.LastPrice, CryptoCurrencyDecimalPlaces),
                            UsdPerDay = Math.Round(x.CoinsPerDay * y.LastPrice * btcUsdValue.Value, FiatDecimalPlaces)
                        })
                        .ToArray()
                })
                .OrderBy(x => x.CoinName)
                .ToArray();
            return new ProfitabilityResponseModel
            {
                Profitabilities = profitabilities
            };
        }
    }
}
