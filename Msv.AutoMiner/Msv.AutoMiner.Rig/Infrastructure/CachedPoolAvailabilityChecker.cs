using System;
using System.Collections.Concurrent;
using Msv.AutoMiner.Common.Infrastructure;
using Msv.AutoMiner.Common.Models.ControlCenterService;

namespace Msv.AutoMiner.Rig.Infrastructure
{
    public class CachedPoolAvailabilityChecker : PoolAvailabilityChecker
    {        
        private static readonly TimeSpan M_RecheckInterval = TimeSpan.FromMinutes(30);

        private readonly ConcurrentDictionary<int, DateTime> m_ResponsesStoppedTimes =
            new ConcurrentDictionary<int, DateTime>();

        public override bool Check(PoolDataModel pool)
        {
            if (m_ResponsesStoppedTimes.TryGetValue(pool.Id, out var stoppedTime)
                && stoppedTime + M_RecheckInterval > DateTime.Now)
            {
                Logger.Warn($"Pool {pool.Name} is still unavailable");
                return false;
            }

            var result = base.Check(pool);
            if (result)
                m_ResponsesStoppedTimes.TryRemove(pool.Id, out _);
            else
                m_ResponsesStoppedTimes.AddOrUpdate(pool.Id, x => DateTime.Now, (x, y) => y);
            return result;
        }
    }
}
