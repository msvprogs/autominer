using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.FrontEnd.Providers
{
    public class PoolInfoProvider : IPoolInfoProvider
    {
        private readonly AutoMinerDbContext m_Context;

        public PoolInfoProvider(AutoMinerDbContext context)
            => m_Context = context;

        public PoolAccountState[] GetCurrentPoolInfos()
            => m_Context.PoolAccountStates
                .AsNoTracking()
                .FromSql(@"SELECT source.* FROM PoolAccountStates source
  JOIN (SELECT PoolId, MAX(DateTime) AS MaxDateTime FROM PoolAccountStates
    GROUP BY PoolId) as grouped
  ON source.PoolId = grouped.PoolId AND source.DateTime = grouped.MaxDateTime")
                .Where(x => x.Pool.Activity != ActivityState.Deleted)
                .ToArray();
    }
}
