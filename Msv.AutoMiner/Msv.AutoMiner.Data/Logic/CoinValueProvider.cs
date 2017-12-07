using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.Data.Logic
{
    public class CoinValueProvider : ICoinValueProvider
    {
        private readonly AutoMinerDbContext m_Context;

        public CoinValueProvider(AutoMinerDbContext context) 
            => m_Context = context;

        public CoinValue[] GetCurrentCoinValues(bool activeOnly)
        {
            var btc = m_Context.Coins.First(x => x.Symbol == "BTC");

            var query = m_Context.ExchangeMarketPrices
                .AsNoTracking()
                .FromSql(@"SELECT source.* FROM ExchangeMarketPrices source
  JOIN (SELECT SourceCoinId, TargetCoinId, Exchange, MAX(DateTime) AS MaxDateTime FROM ExchangeMarketPrices
    GROUP BY SourceCoinId, TargetCoinId, Exchange) as grouped
  ON source.SourceCoinId = grouped.SourceCoinId 
        AND source.TargetCoinId = grouped.TargetCoinId 
        AND source.Exchange = grouped.Exchange 
        AND source.DateTime = grouped.MaxDateTime")
                .Where(x => x.Exchange.Activity == ActivityState.Active)
                .Where(x => x.TargetCoinId == btc.Id);
            query = activeOnly
                ? query.Where(x => x.SourceCoin.Activity == ActivityState.Active)
                : query.Where(x => x.SourceCoin.Activity != ActivityState.Deleted);

            return ToCoinValues(query, btc);
        }

        public CoinValue[] GetAggregatedCoinValues(bool activeOnly, DateTime minDateTime)
        {
            var btc = m_Context.Coins.First(x => x.Symbol == "BTC");

            var query = m_Context.ExchangeMarketPrices
                .AsNoTracking()
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
    WHERE DateTime > '{0:yyyy-MM-dd}'
    GROUP BY SourceCoinId, TargetCoinId, Exchange) as aggregated
  ON source.SourceCoinId = grouped.SourceCoinId 
        AND source.TargetCoinId = grouped.TargetCoinId 
        AND source.Exchange = grouped.Exchange", minDateTime)
                .Where(x => x.Exchange.Activity == ActivityState.Active)
                .Where(x => x.TargetCoinId == btc.Id);
            query = activeOnly
                ? query.Where(x => x.SourceCoin.Activity == ActivityState.Active)
                : query.Where(x => x.SourceCoin.Activity != ActivityState.Deleted);

            return ToCoinValues(query, btc);
        }

        private static CoinValue[] ToCoinValues(IEnumerable<ExchangeMarketPrice> prices, Coin btc) 
            => prices.GroupBy(x => x.SourceCoinId)
                .Select(x => new CoinValue
                {
                    CurrencyId = x.Key,
                    AverageBtcValue = x.Average(y => y.LastPrice),
                    ExchangePrices = x.GroupBy(y => y.ExchangeType)
                        .Select(y => (exchange: y.Key, values: y.OrderByDescending(z => z.DateTime).First()))
                        .Select(y => new CoinExchangePrice
                        {
                            Exchange = y.exchange,
                            Price = y.values.LastPrice,
                            Bid = y.values.HighestBid,
                            Ask = y.values.LowestAsk,
                            Updated = y.values.DateTime
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
