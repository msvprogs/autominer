﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Data.Logic.Contracts;
using Msv.AutoMiner.Data.Logic.Data;

namespace Msv.AutoMiner.Data.Logic
{
    public class CoinValueProvider : ICoinValueProvider
    {
        private readonly IAutoMinerDbContextFactory m_Factory;

        public CoinValueProvider(IAutoMinerDbContextFactory factory)
            => m_Factory = factory;

        public CoinValue[] GetCurrentCoinValues(bool activeOnly, DateTime? dateTime = null)
        {
            using (var context = m_Factory.CreateReadOnly())
            {
                var btc = context.Coins.First(x => x.Symbol == "BTC");

                var query = context.ExchangeMarketPrices
                    .FromSql(@"SELECT source.* FROM ExchangeMarketPrices source
  JOIN (SELECT SourceCoinId, TargetCoinId, Exchange, MAX(DateTime) AS MaxDateTime FROM ExchangeMarketPrices
    WHERE @p0 IS NULL OR DateTime < @p0
    GROUP BY SourceCoinId, TargetCoinId, Exchange) as grouped
  ON source.SourceCoinId = grouped.SourceCoinId 
        AND source.TargetCoinId = grouped.TargetCoinId 
        AND source.Exchange = grouped.Exchange 
        AND source.DateTime = grouped.MaxDateTime",
                        dateTime)
                    .Where(x => x.Exchange.Activity == ActivityState.Active)
                    .Where(x => x.TargetCoinId == btc.Id);
                query = activeOnly
                    ? query.Where(x => x.SourceCoin.Activity == ActivityState.Active)
                    : query.Where(x => x.SourceCoin.Activity != ActivityState.Deleted);

                return ToCoinValues(query, btc, GetIgnoredCurrencies(context));
            }
        }

        public CoinValue[] GetAggregatedCoinValues(bool activeOnly, DateTime minDateTime)
        {
            using (var context = m_Factory.CreateReadOnly())
            {
                var btc = context.Coins.First(x => x.Symbol == "BTC");
                var ignoredCurrencies = context.Exchanges
                    .Where(x => x.Activity == ActivityState.Active)
                    .ToDictionary(x => x.Type, x => x.IgnoredCurrencies.EmptyIfNull().Split(','));

                var query = context.ExchangeMarketPrices
                    .FromSql(@"SELECT source.SourceCoinId, source.TargetCoinId, source.Exchange, source.DateTime, 
  source.BuyFeePercent, aggregated.AvgHighestBid AS HighestBid,
  source.IsActive, source.LastDayHigh, source.LastDayLow, source.LastDayVolume, 
  aggregated.AvgLastPrice AS LastPrice, aggregated.AvgLowestAsk AS LowestAsk, source.SellFeePercent
  FROM ExchangeMarketPrices source
  JOIN (SELECT SourceCoinId, TargetCoinId, Exchange, MAX(DateTime) AS MaxDateTime FROM ExchangeMarketPrices
    GROUP BY SourceCoinId, TargetCoinId, Exchange) as grouped
    ON source.SourceCoinId = grouped.SourceCoinId 
        AND source.TargetCoinId = grouped.TargetCoinId 
        AND source.Exchange = grouped.Exchange 
        AND source.DateTime = grouped.MaxDateTime
  JOIN (SELECT SourceCoinId, TargetCoinId, Exchange, AVG(HighestBid) as AvgHighestBid, AVG(LowestAsk) AS AvgLowestAsk, AVG(LastPrice) as AvgLastPrice 
    FROM ExchangeMarketPrices
    WHERE DateTime > @p0
    GROUP BY SourceCoinId, TargetCoinId, Exchange) as aggregated
  ON source.SourceCoinId = aggregated.SourceCoinId 
        AND source.TargetCoinId = aggregated.TargetCoinId 
        AND source.Exchange = aggregated.Exchange", minDateTime)
                    .Where(x => x.Exchange.Activity == ActivityState.Active)
                    .Where(x => x.TargetCoinId == btc.Id);
                query = activeOnly
                    ? query.Where(x => x.SourceCoin.Activity == ActivityState.Active)
                    : query.Where(x => x.SourceCoin.Activity != ActivityState.Deleted);

                return ToCoinValues(query, btc, GetIgnoredCurrencies(context));
            }
        }

        private static Dictionary<ExchangeType, Guid[]> GetIgnoredCurrencies(AutoMinerDbContext context)
        {
            var ignoredCurrencies = context.Exchanges
                .Where(x => x.Activity == ActivityState.Active)
                .ToDictionary(x => x.Type, x => x.IgnoredCurrencies.EmptyIfNull().Split(','));
            var allIgnoredSymbols = ignoredCurrencies.SelectMany(x => x.Value).Distinct().ToArray();
            var allIgnoredCoins = context.Coins
                .Where(x => x.Activity != ActivityState.Deleted)
                .Where(x => allIgnoredSymbols.Contains(x.Symbol))
                .Select(x => new {x.Symbol, x.Id})
                .ToArray();
            return ignoredCurrencies.ToDictionary(x => x.Key, x => x.Value
                .Join(allIgnoredCoins, y => y, y => y.Symbol, (y, z) => z.Id)
                .ToArray());
        }

        private static CoinValue[] ToCoinValues(
            IEnumerable<ExchangeMarketPrice> prices, Coin btc, Dictionary<ExchangeType, Guid[]> ignoredCurrencies) 
            => prices.GroupBy(x => x.SourceCoinId)
                .Select(x => new CoinValue
                {
                    CurrencyId = x.Key,
                    AverageBtcValue = x.Average(y => y.LastPrice),
                    ExchangePrices = x.GroupBy(y => y.ExchangeType)
                        .Select(y => (exchange: y.Key, values: y.OrderByDescending(z => z.DateTime).First()))
                        .Where(y => !(ignoredCurrencies.TryGetValue(y.exchange)?.Contains(y.values.SourceCoinId)).GetValueOrDefault())
                        .Select(y => new CoinExchangePrice
                        {
                            Exchange = y.exchange,
                            Price = y.values.LastPrice,
                            Bid = y.values.HighestBid,
                            Ask = y.values.LowestAsk,
                            Volume = y.values.LastDayVolume,
                            Updated = y.values.DateTime,
                            IsActive = y.values.IsActive
                        })
                        .ToArray()
                })
                .Concat(new[] {new CoinValue
                {
                    CurrencyId = btc.Id,
                    AverageBtcValue = 1
                }})
                .ToArray();
    }
}
