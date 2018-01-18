using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.Data.Logic
{
    public class CoinNetworkInfoProvider : ICoinNetworkInfoProvider
    {
        private readonly AutoMinerDbContext m_Context;

        public CoinNetworkInfoProvider(AutoMinerDbContext context)
            => m_Context = context;

        public CoinNetworkInfo[] GetCurrentNetworkInfos(bool activeOnly, DateTime? dateTime = null)
        {
            var query = m_Context.CoinNetworkInfos
                .Include(x => x.Coin)
                .Include(x => x.Coin.Algorithm)
                .AsNoTracking()
                .FromSql(@"SELECT source.* FROM CoinNetworkInfos source
  JOIN (SELECT CoinId, MAX(Created) AS MaxCreated FROM CoinNetworkInfos
  WHERE @p0 IS NULL OR Created < @p0
  GROUP BY CoinId) AS grouped
  ON source.CoinId = grouped.CoinId AND source.Created = grouped.MaxCreated", dateTime);
            query = activeOnly 
                ? query.Where(x => x.Coin.Activity == ActivityState.Active)
                : query.Where(x => x.Coin.Activity != ActivityState.Deleted);
            return query.ToArray();
        }

        public CoinNetworkInfo[] GetAggregatedNetworkInfos(bool activeOnly, DateTime minDateTime)
        {
            var query = m_Context.CoinNetworkInfos
                .Include(x => x.Coin)
                .Include(x => x.Coin.Algorithm)
                .AsNoTracking()
                .FromSql(
                    @"SELECT source.CoinId, source.Created, source.BlockReward, source.BlockTimeSeconds, aggregated.AvgDifficulty AS Difficulty, source.Height, source.NetHashRate
  FROM CoinNetworkInfos source
  JOIN (SELECT CoinId, AVG(Difficulty) AS AvgDifficulty FROM CoinNetworkInfos
    WHERE Created > @p0
    GROUP BY CoinId) AS aggregated
  ON source.CoinId = aggregated.CoinId
  JOIN (SELECT CoinId, MAX(Created) AS MaxCreated FROM CoinNetworkInfos
    GROUP BY CoinId) AS grouped
    ON source.CoinId = grouped.CoinId AND source.Created = grouped.MaxCreated", minDateTime);
            query = activeOnly
                ? query.Where(x => x.Coin.Activity == ActivityState.Active)
                : query.Where(x => x.Coin.Activity != ActivityState.Deleted);
            return query.ToArray();
        }
    }
}