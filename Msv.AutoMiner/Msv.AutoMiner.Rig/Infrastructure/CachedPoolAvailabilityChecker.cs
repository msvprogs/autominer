using System;
using System.Collections.Concurrent;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Infrastructure;
using Msv.AutoMiner.Common.Models.ControlCenterService;

namespace Msv.AutoMiner.Rig.Infrastructure
{
    public class CachedPoolAvailabilityChecker : PoolAvailabilityChecker
    {        
        private static readonly TimeSpan M_RecheckInterval = TimeSpan.FromMinutes(30);

        private readonly ConcurrentDictionary<int, DateTime> m_ResponsesStoppedTimes =
            new ConcurrentDictionary<int, DateTime>();

        public CachedPoolAvailabilityChecker(IWebClient webClient) 
            : base(webClient)
        { }

        public override PoolAvailabilityState Check(PoolDataModel pool, KnownCoinAlgorithm? knownCoinAlgorithm)
        {
            if (m_ResponsesStoppedTimes.TryGetValue(pool.Id, out var stoppedTime)
                && stoppedTime + M_RecheckInterval > DateTime.Now)
            {
                Logger.Warn($"Pool {pool.Name} is still unavailable");
                return PoolAvailabilityState.NoResponse;
            }

            var result = base.Check(pool, knownCoinAlgorithm);
            if (result == PoolAvailabilityState.Available)
                m_ResponsesStoppedTimes.TryRemove(pool.Id, out _);
            else
                m_ResponsesStoppedTimes.AddOrUpdate(pool.Id, x => DateTime.Now, (x, y) => y);
            return result;
        }
    }
}
