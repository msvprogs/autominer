using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            var coinIds = await m_Context.Coins
                .AsNoTracking()
                .Where(x => x.Activity == ActivityState.Active)
                .Select(x => x.Id)
                .ToArrayAsync();
            var maxDate = await m_Context.CoinNetworkInfos.MaxAsync(x => x.Created);
            var lastNetworkInfos = await m_Context.CoinNetworkInfos
                .Include(x => x.Coin)
                .Include(x => x.Coin.Algorithm)
                .AsNoTracking()
                .Where(x => coinIds.Contains(x.CoinId) && x.Created == maxDate)
                .ToArrayAsync();

            if (aggregationType == ValueAggregationType.Last)
                return lastNetworkInfos;
            var to = DateTime.UtcNow;
            var from = GetMinDateTime(aggregationType);
            return m_Context.CoinNetworkInfos
                .AsNoTracking()
                .Where(x => coinIds.Contains(x.CoinId))
                .Where(x => x.Created >= from && x.Created <= to)
                .Select(x => new { x.CoinId, x.Difficulty })
                .AsEnumerable()
                .GroupBy(x => x.CoinId)
                .Select(x => new
                {
                    CoinId = x.Key,
                    Difficulty = x.Average(y => y.Difficulty)
                })
                .Join(lastNetworkInfos, x => x.CoinId, x => x.CoinId, (x, y) =>
                {
                    y.Difficulty = x.Difficulty;
                    return y;
                })
                .ToArray();
        }

        public async Task<ExchangeMarketPrice[]> GetExchangeMarketPrices(ValueAggregationType aggregationType)
        {
            var maxDate = await m_Context.ExchangeMarketPrices.Select(x => x.DateTime).MaxAsync();
            var lastPrices = await m_Context.ExchangeMarketPrices
                .AsNoTracking()
                .Where(x => x.SourceCoin.Activity == ActivityState.Active && x.DateTime == maxDate)
                .ToArrayAsync();
            if (aggregationType == ValueAggregationType.Last)
                return lastPrices;
            var coinIds = lastPrices.Select(x => x.SourceCoinId).ToArray();
            var to = DateTime.UtcNow;
            var from = GetMinDateTime(aggregationType);
            return m_Context.ExchangeMarketPrices
                .AsNoTracking()
                .Where(x => coinIds.Contains(x.SourceCoinId))
                .Where(x => x.DateTime >= from && x.DateTime <= to)
                .Select(x => new { x.SourceCoinId, x.TargetCoinId, x.Exchange, x.HighestBid, x.LowestAsk, x.LastPrice })
                .AsEnumerable()
                .GroupBy(x => new {x.SourceCoinId, x.TargetCoinId, x.Exchange})
                .Select(x => new
                {
                    x.Key,
                    Bid = x.Average(y => y.HighestBid),
                    Ask = x.Average(y => y.LowestAsk),
                    Last = x.Average(y => y.LastPrice)
                })
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
