using System;
using System.Linq;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.Infrastructure;
using Msv.AutoMiner.Common.Models.CoinInfoService;

namespace Msv.AutoMiner.Data.Logic
{
    public class ProfitabilityTableBuilder : IProfitabilityTableBuilder
    {
        private const int CryptoCurrencyDecimalPlaces = 8;
        private const int FiatDecimalPlaces = 4;
        
        private readonly IStoredFiatValueProvider m_FiatProvider;
        private readonly IProfitabilityCalculator m_Calculator;
        private readonly ICoinNetworkInfoProvider m_CoinNetworkInfoProvider;
        private readonly ICoinValueProvider m_CoinValueProvider;

        public ProfitabilityTableBuilder(
            IStoredFiatValueProvider fiatProvider, 
            IProfitabilityCalculator calculator,
            ICoinNetworkInfoProvider coinNetworkInfoProvider, 
            ICoinValueProvider coinValueProvider)
        {
            m_FiatProvider = fiatProvider ?? throw new ArgumentNullException(nameof(fiatProvider));
            m_Calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
            m_CoinNetworkInfoProvider = coinNetworkInfoProvider ?? throw new ArgumentNullException(nameof(coinNetworkInfoProvider));
            m_CoinValueProvider = coinValueProvider ?? throw new ArgumentNullException(nameof(coinValueProvider));
        }

        public SingleProfitabilityData[] Build(ProfitabilityRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var networkInfos = request.DifficultyAggregationType == ValueAggregationType.Last
                ? m_CoinNetworkInfoProvider.GetCurrentNetworkInfos(true)
                : m_CoinNetworkInfoProvider.GetAggregatedNetworkInfos(
                    true, GetMinDateTime(request.DifficultyAggregationType));
            var marketPrices = request.PriceAggregationType == ValueAggregationType.Last
                ? m_CoinValueProvider.GetCurrentCoinValues(true)
                : m_CoinValueProvider.GetAggregatedCoinValues(true, GetMinDateTime(request.PriceAggregationType));

            var btcUsdValue = m_FiatProvider.GetLastBtcUsdValue();
            return request.AlgorithmDatas
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
        }

        public EstimateProfitabilityResponse EstimateProfitability(EstimateProfitabilityRequest request)
        {
            if (request == null) 
                throw new ArgumentNullException(nameof(request));

            var btcUsdValue = m_FiatProvider.GetLastBtcUsdValue();
            var coinsPerDay = m_Calculator.CalculateCoinsPerDay(
                request.Difficulty,
                request.BlockReward,
                request.MaxTarget,
                request.ClientHashRate);
            var btcPerDay = coinsPerDay * request.BtcPrice;
            var electricityCostPerDay = GetElectricityCostPerDay(request.ClientPowerUsage, request.ElectricityCostUsd);
            return new EstimateProfitabilityResponse
            {
                Coins = new EstimateProfitabilityResponse.CumulativeProfitability(coinsPerDay),
                Btc = new EstimateProfitabilityResponse.CumulativeProfitability(btcPerDay),
                Usd = new EstimateProfitabilityResponse.CumulativeProfitability(
                    btcPerDay * btcUsdValue.Value - electricityCostPerDay)
            };
        }

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

        private static double GetElectricityCostPerDay(double unitCost, double powerUsageWatts)
            => Math.Round(powerUsageWatts / 1000 * unitCost * 24, FiatDecimalPlaces);
    }
}
