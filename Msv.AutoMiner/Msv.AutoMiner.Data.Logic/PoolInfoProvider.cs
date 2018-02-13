using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data.Logic.Contracts;

namespace Msv.AutoMiner.Data.Logic
{
    public class PoolInfoProvider : IPoolInfoProvider
    {
        private readonly IAutoMinerDbContextFactory m_Factory;

        public PoolInfoProvider(IAutoMinerDbContextFactory factory)
            => m_Factory = factory;

        public PoolAccountState[] GetCurrentPoolInfos()
        {
            using (var context = m_Factory.Create())
                return context.PoolAccountStates
                    .AsNoTracking()
                    .FromSql(@"SELECT source.* FROM PoolAccountStates source
      JOIN (SELECT PoolId, MAX(DateTime) AS MaxDateTime FROM PoolAccountStates
        GROUP BY PoolId) as grouped
      ON source.PoolId = grouped.PoolId AND source.DateTime = grouped.MaxDateTime")
                    .Where(x => x.Pool.Activity != ActivityState.Deleted)
                    .ToArray();
        }
    }
}
