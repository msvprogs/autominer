using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.FrontEnd.Providers
{
    public class PoolInfoProvider : IPoolInfoProvider
    {
        private static readonly TimeSpan M_MinDatePeriod = TimeSpan.FromDays(4);

        private readonly AutoMinerDbContext m_Context;

        public PoolInfoProvider(AutoMinerDbContext context)
            => m_Context = context;

        public PoolAccountState[] GetCurrentPoolInfos()
        {
            var minDate = DateTime.UtcNow - M_MinDatePeriod;
            var maxDates = m_Context.PoolAccountStates
                .AsNoTracking()
                .Where(x => x.Pool.Activity != ActivityState.Deleted)
                .Where(x => x.DateTime > minDate)
                .Select(x => new { x.PoolId, x.DateTime })
                .AsEnumerable()
                .GroupBy(x => x.PoolId)
                .Select(x => x.OrderByDescending(y => y.DateTime).First().DateTime)
                .Distinct()
                .ToArray();

            return m_Context.PoolAccountStates
                .AsNoTracking()
                .Where(x => x.Pool.Activity != ActivityState.Deleted)
                .Where(x => maxDates.Contains(x.DateTime))
                .AsEnumerable()
                .GroupBy(x => x.PoolId)
                .Select(x => x.OrderByDescending(y => y.DateTime).First())
                .ToArray();
        }
    }
}
