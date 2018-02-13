using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Data.Logic.Contracts;

namespace Msv.AutoMiner.Data.Logic
{
    public class CoinNetworkInfoProvider : ICoinNetworkInfoProvider
    {
        private readonly IAutoMinerDbContextFactory m_Factory;

        public CoinNetworkInfoProvider(IAutoMinerDbContextFactory factory)
            => m_Factory = factory;

        public CoinNetworkInfo[] GetCurrentNetworkInfos(bool activeOnly, DateTime? dateTime = null)
        {
            using (var context = m_Factory.CreateReadOnly())
            {
                var query = context.CoinNetworkInfos
                    .Include(x => x.Coin)
                    .Include(x => x.Coin.Algorithm)
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
        }

        public CoinNetworkInfo[] GetAggregatedNetworkInfos(bool activeOnly, DateTime minDateTime)
        {
            using (var context = m_Factory.CreateReadOnly())
            {
                var query = context.CoinNetworkInfos
                    .Include(x => x.Coin)
                    .Include(x => x.Coin.Algorithm)
                    .FromSql(@"
SELECT source.CoinId, source.Created, source.BlockReward, source.BlockTimeSeconds, aggregated.AvgDifficulty AS Difficulty,
        source.Height, source.NetHashRate, source.LastBlockTime, source.MasternodeCount, source.TotalSupply
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
}