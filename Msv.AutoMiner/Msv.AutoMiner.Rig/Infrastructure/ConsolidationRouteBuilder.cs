//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Msv.AutoMiner.Commons;
//using Msv.AutoMiner.Service.Data;
//using Msv.AutoMiner.Service.Infrastructure.Contracts;
//using Msv.AutoMiner.Service.Infrastructure.Data;
//using Msv.AutoMiner.Service.Storage.Contracts;

//namespace Msv.AutoMiner.Service.Infrastructure
//{
//    public class ConsolidationRouteBuilder : IConsolidationRouteBuilder
//    {
//        private readonly IConsolidationRouteBuilderStorage m_Storage;

//        public ConsolidationRouteBuilder(IConsolidationRouteBuilderStorage storage)
//        {
//            if (storage == null)
//                throw new ArgumentNullException(nameof(storage));

//            m_Storage = storage;
//        }

//        public Dictionary<ExchangeType, Dictionary<ExchangeType, ConsolidationRoute[]>> BuildForBtc(
//            Dictionary<ExchangeType, Dictionary<string, CoinMarketInfo>> marketValues)
//        {
//            if (marketValues == null)
//                throw new ArgumentNullException(nameof(marketValues));

//            var btcBalances = m_Storage.GetBitCoinBalances();
//            return marketValues
//                .Where(x => x.Value != null)
//                .SelectMany(x => marketValues.Where(y => y.Value != null), (x, y) => new
//                {
//                    Source = x.Key,
//                    Target = y.Key,
//                    SourceBalance = btcBalances.TryGetValue(x.Key, 1),
//                    Currencies = x.Value.Join(y.Value, z => z.Key, z => z.Key, (a, b) => new
//                        {
//                            Symbol = a.Key,
//                            a.Value.WithdrawalFee,
//                            SourcePrice = a.Value.BtcLowestAsk,
//                            SourceConvFee = a.Value.ConversionFeePercent,
//                            TargetPrice = b.Value.BtcHighestBid,
//                            TargetConvFee = b.Value.ConversionFeePercent
//                        })
//                        .ToArray()
//                })
//                .Where(x => x.Source != x.Target && x.SourceBalance > 0 && x.Currencies.Any())
//                .Select(x => new
//                {
//                    x.Source,
//                    x.Target,
//                    ConversionResult = x.Currencies
//                        .Where(y => y.WithdrawalFee != null && y.SourcePrice > 0)
//                        .Select(y => new
//                        {
//                            Currency = y,
//                            x.SourceBalance,
//                            SourceCoinAmount = x.SourceBalance * (1 - y.SourceConvFee / 100) / y.SourcePrice
//                        })
//                        .Select(y => new
//                        {
//                            y.Currency,
//                            y.SourceBalance,
//                            y.SourceCoinAmount,
//                            TargetCoinAmount = y.SourceCoinAmount - y.Currency.WithdrawalFee.Value
//                        })
//                        .Select(y => new ConsolidationRoute
//                        {
//                            CurrencySymbol = y.Currency.Symbol,
//                            SourceCoinAmount = y.SourceCoinAmount,
//                            SourceBtcAmount = y.SourceBalance,
//                            TargetCoinAmount = y.TargetCoinAmount,
//                            TargetBtcAmount = y.TargetCoinAmount * y.Currency.TargetPrice
//                                              * (1 - y.Currency.TargetConvFee / 100)
//                        })
//                        .OrderByDescending(y => y.TargetBtcAmount)
//                        .Take(3)
//                        .Concat(new[]
//                        {
//                            new ConsolidationRoute
//                            {
//                                CurrencySymbol = "No conv",
//                                SourceBtcAmount = x.SourceBalance,
//                                TargetBtcAmount = (x.SourceBalance - x.Currencies.FirstOrDefault(y => y.Symbol == "BTC")?.WithdrawalFee).GetValueOrDefault()
//                            }
//                        })
//                        .OrderByDescending(y => y.TargetBtcAmount)
//                        .ToArray()
//                })
//                .GroupBy(x => x.Source)
//                .ToDictionary(x => x.Key, x => x.ToDictionary(y => y.Target, y => y.ConversionResult));
//        }
//    }
//}
