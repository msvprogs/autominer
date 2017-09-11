using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Msv.AutoMiner.Commons;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Msv.AutoMiner.Service.Infrastructure.Contracts;
using Msv.AutoMiner.Service.Infrastructure.Data;
using Msv.AutoMiner.Service.Storage.Contracts;
using NLog;

namespace Msv.AutoMiner.Service.Infrastructure
{
    public class ProfitabilityTableBuilder : IProfitabilityTableBuilder
    {
        private static readonly ILogger M_Logger = LogManager.GetLogger("ProfitabilityTableBuilder");

        private readonly ICoinNetworkInfoUpdater m_NetworkInfoUpdater;
        private readonly IMarketValuesProvider m_MarketValuesProvider;
        private readonly ICoinMarketInfoAggregator m_MarketInfoAggregator;
        private readonly IConsolidationRouteBuilder m_ConsolidationRouteBuilder;
        private readonly IProfitabilityCalculator m_ProfitabilityCalculator;
        private readonly IBitCoinMarketPriceProvider m_BitCoinMarketPriceProvider;
        private readonly IProfitabilityTableBuilderStorage m_Storage;
        private readonly ProfitabilityTableBuilderParams m_Parameters;

        public ProfitabilityTableBuilder(
            ICoinNetworkInfoUpdater networkInfoUpdater,
            IMarketValuesProvider marketValuesProvider,
            ICoinMarketInfoAggregator marketInfoAggregator, 
            IConsolidationRouteBuilder consolidationRouteBuilder,
            IProfitabilityCalculator profitabilityCalculator, 
            IBitCoinMarketPriceProvider bitCoinMarketPriceProvider,
            IProfitabilityTableBuilderStorage storage,
            ProfitabilityTableBuilderParams parameters)
        {
            if (networkInfoUpdater == null)
                throw new ArgumentNullException(nameof(networkInfoUpdater));
            if (marketValuesProvider == null)
                throw new ArgumentNullException(nameof(marketValuesProvider));
            if (marketInfoAggregator == null)
                throw new ArgumentNullException(nameof(marketInfoAggregator));
            if (consolidationRouteBuilder == null)
                throw new ArgumentNullException(nameof(consolidationRouteBuilder));
            if (profitabilityCalculator == null)
                throw new ArgumentNullException(nameof(profitabilityCalculator));
            if (bitCoinMarketPriceProvider == null)
                throw new ArgumentNullException(nameof(bitCoinMarketPriceProvider));
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            m_NetworkInfoUpdater = networkInfoUpdater;
            m_MarketValuesProvider = marketValuesProvider;
            m_MarketInfoAggregator = marketInfoAggregator;
            m_ConsolidationRouteBuilder = consolidationRouteBuilder;
            m_ProfitabilityCalculator = profitabilityCalculator;
            m_BitCoinMarketPriceProvider = bitCoinMarketPriceProvider;
            m_Storage = storage;
            m_Parameters = parameters;
        }

        public CoinProfitabilityData[] Build()
        {
            M_Logger.Info("Updating coin network statistics...");
            var coins = m_NetworkInfoUpdater.UpdateNetworkInfo();
            M_Logger.Info("Coin network statistics updated. Obtaining current coin market values...");
            var marketValues = m_MarketValuesProvider.GetCoinMarketValues(coins);
            M_Logger.Info("Current coin market values were received. Aggregating them...");
            var aggregatedValues = m_MarketInfoAggregator.GetAggregatedMarketPrices(marketValues);
            M_Logger.Info("Coin market value aggregation complete. Building BTC consolidation routes...");
            var consolidationRoutes = m_ConsolidationRouteBuilder.BuildForBtc(marketValues);
            M_Logger.Info("BTC consolidation routes building completed.");
            M_Logger.Info(
                "============[BTC Consolidation information]============"
                + string.Join(Environment.NewLine, consolidationRoutes.Select(
                    x => $"{Environment.NewLine}{Environment.NewLine}{x.Key}:{Environment.NewLine}"
                         + string.Join(Environment.NewLine, x.Value.Select(y =>
                             $"\t->{y.Key} - {Environment.NewLine}\t\t" + string.Join(Environment.NewLine + "\t\t", y.Value.Select(
                                 z => $"{z.SourceBtcAmount:N5} BTC -> {z.SourceCoinAmount:N5} {z.CurrencySymbol} "
                                      + $"-> {z.TargetCoinAmount:N5} {z.CurrencySymbol} -> {z.TargetBtcAmount:N5} BTC")))))));

            var bitCoinInUsd = m_BitCoinMarketPriceProvider.GetCurrentPriceUsd();

            M_Logger.Info($"Current BTC price is 1 BTC = ${bitCoinInUsd:N2}");
            var profitabilityTable = BuildProfitabilityTable(coins, bitCoinInUsd, aggregatedValues);
            M_Logger.Debug("Current profitability table:"
                           + Environment.NewLine
                           + "Enabled Symbol       Name                                   Coins per day   BTC per day   USD per day  Electricity  Total"
                           + Environment.NewLine
                           + string.Join(Environment.NewLine, profitabilityTable.Select(
                               x => new StringBuilder((x.Coins.First().Coin.Activity == ActivityState.Active ? "+" : "-") + "     \t")
                                   .Append(x.ToCoinString().PadRight(12))
                                   .Append(x.ToCoinNameString().PadRight(30))
                                   .Append(x.ToCoinsPerDayString().PadLeft(20))
                                   .Append($"{x.BtcPerDay,18:N6}")
                                   .Append($"{x.UsdPerDay,10:N2}")
                                   .Append($"{-x.ElectricityCost,13:N2}$")
                                   .Append($"{x.UsdPerDayTotal,10:N2}$"))));
            return profitabilityTable;
        }

        private CoinProfitabilityData[] BuildProfitabilityTable(
            IEnumerable<Coin> coins,
            double bitCoinInUsd,
            Dictionary<string, double> marketValues)
        {
            var algorithmDatas = m_Storage.GetAlgorithmDatas()
                .ToDictionary(x => x.Algorithm);
            var algorithmPairDatas = m_Storage.GetAlgorithmPairDatas()
                .Where(x => x.IsActive)
                .ToDictionary(
                    x => Tuple.Create(x.Algorithm1, x.Algorithm2));
            var coinMarketInfos = coins
                .Where(x => x.Pool != null)
                .Select(x => new
                {
                    Coin = x,
                    HashRate = algorithmDatas.TryGetValue(x.Algorithm)?.SpeedInHashes,
                    PowerUsage = algorithmDatas.TryGetValue(x.Algorithm)?.Power,
                    BtcPrice = marketValues.TryGetValue(x.CurrencySymbol)
                })
                .Where(x => x.HashRate != null && x.BtcPrice > 0)
                .Select(x => new
                {
                    x.Coin,
                    HashRate = x.HashRate.Value,
                    PowerUsage = x.PowerUsage.Value,
                    x.BtcPrice
                })
                .ToArray();
            var pairableMarketInfos = coinMarketInfos.Join(algorithmPairDatas.Keys
                        .SelectMany(x => new[] {x.Item1, x.Item2}).Distinct(),
                    x => x.Coin.Algorithm,
                    x => x,
                    (x, y) => x)
                .ToArray();
            return pairableMarketInfos.SelectMany(x => pairableMarketInfos, Tuple.Create)
                .Select(x => new
                {
                    Data = x,
                    PairInfo = algorithmPairDatas.TryGetValue(Tuple.Create(x.Item1.Coin.Algorithm,
                        x.Item2.Coin.Algorithm))
                })
                .Where(x => x.PairInfo != null)
                .Select(x => new
                {
                    Coin1 = x.Data.Item1.Coin,
                    Coin2 = x.Data.Item2.Coin,
                    BtcPrice1 = x.Data.Item1.BtcPrice,
                    BtcPrice2 = x.Data.Item2.BtcPrice,
                    x.PairInfo,
                    CoinsPerDay1 = CalculateCoinsPerDay(x.Data.Item1.Coin, x.PairInfo.SpeedInHashes1),
                    CoinsPerDay2 = CalculateCoinsPerDay(x.Data.Item2.Coin, x.PairInfo.SpeedInHashes2)
                })
                .Select(x => new CoinProfitabilityData
                {
                    Mode = MiningMode.Double,
                    Coins = new[]
                    {
                        new SingleCoinProfitability(x.Coin1, x.CoinsPerDay1),
                        new SingleCoinProfitability(x.Coin2, x.CoinsPerDay2)
                    },
                    Miner = x.PairInfo.Miner,
                    BtcPerDay = x.CoinsPerDay1 * x.BtcPrice1 + x.CoinsPerDay2 * x.BtcPrice2,
                    PowerUsage = x.PairInfo.Power
                })
                .Concat(coinMarketInfos
                    .Select(x => new
                    {
                        x.Coin,
                        x.BtcPrice,
                        x.PowerUsage,
                        CoinsPerDay = CalculateCoinsPerDay(x.Coin, x.HashRate)
                    })
                    .GroupBy(x => x.Coin.Pool.Id)
                    .Select(x => new CoinProfitabilityData
                    {
                        Coins = x.Select(y => new SingleCoinProfitability(y.Coin, y.CoinsPerDay)).ToArray(),
                        Mode = x.Count() > 1 ? MiningMode.Merged : MiningMode.Single,
                        Miner = x.First().Coin.Pool.Miner,
                        BtcPerDay = x.Sum(y => y.CoinsPerDay * y.BtcPrice),
                        PowerUsage = x.First().PowerUsage
                    }))
                .Do(x => x.UsdPerDay = (decimal) (x.BtcPerDay * bitCoinInUsd))
                .Do(x => x.ElectricityCost = (decimal)((x.PowerUsage + m_Parameters.SystemPowerUsageWatts) / 1000 * m_Parameters.ElectricityKwhCostUsd * 24))
                .OrderByDescending(x => x.UsdPerDayTotal)
                .ToArray();
        }

        private double CalculateCoinsPerDay(Coin coin, long hashRate)
            => m_ProfitabilityCalculator.CalculateCoinsPerDay(coin, hashRate) * (1 - (double)coin.Pool.FeeRatio / 100);
    }
}
