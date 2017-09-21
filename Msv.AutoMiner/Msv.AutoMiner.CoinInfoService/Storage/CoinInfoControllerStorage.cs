using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Storage
{
    public class CoinInfoControllerStorage : ICoinInfoControllerStorage
    {
        private readonly AutoMinerDbContext m_Context;

        public CoinInfoControllerStorage(AutoMinerDbContext context)
        {
            m_Context = context;
        }

        public async Task<CoinAlgorithm[]> GetAlgorithms()
            => await m_Context.CoinAlgorithms
                .AsNoTracking()
                .ToArrayAsync();

        public async Task<CoinNetworkInfo[]> GetNetworkInfos(ValueAggregationType aggregationType)
        {
            var coins = await m_Context.Coins
                .Include(x => x.Algorithm)
                .AsNoTracking()
                .Where(x => x.Activity != ActivityState.Deleted)
                .ToDictionaryAsync(x => x.Id);
            var coinIds = coins.Select(x => x.Key).ToArray();
            var lastNetworkInfos = await m_Context.CoinNetworkInfos
                .AsNoTracking()
                .Where(x => coinIds.Contains(x.CoinId))
                .GroupBy(x => x.CoinId)
                .Select(x => x.OrderByDescending(y => y.Created).FirstOrDefault())
                .ToArrayAsync();
            lastNetworkInfos.ForEach(x => x.Coin = coins[x.CoinId]);

            if (aggregationType == ValueAggregationType.Last)
                return lastNetworkInfos;
            var to = DateTime.UtcNow;
            var from = GetMinDateTime(aggregationType);
            return (await m_Context.CoinNetworkInfos
                    .AsNoTracking()
                    .Where(x => coinIds.Contains(x.CoinId))
                    .Where(x => x.Created >= from && x.Created <= to)
                    .GroupBy(x => x.CoinId)
                    .Select(x => new
                    {
                        CoinId = x.Key,
                        Difficulty = x.Average(y => y.Difficulty)
                    })
                    .ToArrayAsync())
                .Join(lastNetworkInfos, x => x.CoinId, x => x.CoinId, (x, y) =>
                {
                    y.Difficulty = x.Difficulty;
                    return y;
                })
                .ToArray();
        }

        public async Task<ExchangeMarketPrice[]> GetExchangeMarketPrices(ValueAggregationType aggregationType)
        {
            var lastPrices = await m_Context.ExchangeMarketPrices
                .AsNoTracking()
                .Where(x => x.SourceCoin.Activity != ActivityState.Deleted)
                .GroupBy(x => new {x.SourceCoinId, x.TargetCoinId, x.Exchange})
                .Select(x => x.OrderByDescending(y => y.DateTime).FirstOrDefault())
                .ToArrayAsync();
            if (aggregationType == ValueAggregationType.Last)
                return lastPrices;
            var coinIds = lastPrices.Select(x => x.SourceCoinId).ToArray();
            var to = DateTime.UtcNow;
            var from = GetMinDateTime(aggregationType);
            return (await m_Context.ExchangeMarketPrices
                    .AsNoTracking()
                    .Where(x => coinIds.Contains(x.SourceCoinId))
                    .Where(x => x.DateTime >= from && x.DateTime <= to)
                    .GroupBy(x => new {x.SourceCoinId, x.TargetCoinId, x.Exchange})
                    .Select(x => new
                    {
                        x.Key,
                        Bid = x.Average(y => y.HighestBid),
                        Ask = x.Average(y => y.LowestAsk),
                        Last = x.Average(y => y.LastPrice)
                    })
                    .ToArrayAsync())
                .Join(lastPrices, x => x.Key, x => new {x.SourceCoinId, x.TargetCoinId, x.Exchange}, (x, y) =>
                {
                    y.HighestBid = x.Bid;
                    y.LowestAsk = x.Ask;
                    y.LastPrice = x.Last;
                    return y;
                })
                .ToArray();
        }

        public async Task<Coin> GetBtcCurrency()
            => await m_Context.Coins.AsNoTracking().FirstAsync(x => x.Symbol == "BTC");

        public async Task<CoinFiatValue> GetBtcUsdValue()
            => await m_Context.CoinFiatValues
                .AsNoTracking()
                .Where(x => x.Coin.Symbol == "BTC" && x.FiatCurrency.Symbol == "USD")
                .GroupBy(x => new {x.CoinId, x.FiatCurrencyId})
                .Select(x => x.OrderByDescending(y => y.DateTime).FirstOrDefault())
                .FirstAsync();

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
