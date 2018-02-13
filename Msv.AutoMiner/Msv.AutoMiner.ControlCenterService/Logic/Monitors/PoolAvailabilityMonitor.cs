using System;
using System.Linq;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.Infrastructure;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts;
using Msv.AutoMiner.Data.Logic;

namespace Msv.AutoMiner.ControlCenterService.Logic.Monitors
{
    public class PoolAvailabilityMonitor : MonitorBase
    {        
        private const int ParallelismDegree = 6;

        private readonly IPoolAvailabilityChecker m_PoolAvailabilityChecker;
        private readonly IPoolAvailabilityMonitorStorage m_Storage;

        public PoolAvailabilityMonitor(
            IPoolAvailabilityChecker poolAvailabilityChecker,
            IPoolAvailabilityMonitorStorage storage)
            : base(TimeSpan.FromMinutes(30))
        {
            m_PoolAvailabilityChecker = poolAvailabilityChecker ?? throw new ArgumentNullException(nameof(poolAvailabilityChecker));
            m_Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        protected override void DoWork()
        {
            var now = DateTime.UtcNow;
            var btcMiningTarget = m_Storage.GetBitCoinMiningTarget();
            var results = m_Storage.GetActivePools()
                .AsParallel()
                .WithDegreeOfParallelism(ParallelismDegree)
                .Select(x => (pool:x, login: x.GetLogin(x.UseBtcWallet ? btcMiningTarget : x.Coin.Wallets.FirstOrDefault(y => y.IsMiningTarget))))
                .Where(x => x.login != null)
                .Select(x => new
                {
                    Pool = x.pool,
                    Availability = m_PoolAvailabilityChecker.Check(
                        new PoolDataModel
                        {
                            Login = x.login,
                            Password = x.pool.WorkerPassword,
                            Name = x.pool.Name,
                            Protocol = x.pool.Protocol,
                            Url = x.pool.GetUrl()
                        },
                        x.pool.Coin.Algorithm.KnownValue)
                })
                .Where(x => x.Pool.Availability != x.Availability)
                .ToDictionary(
                    x => x.Pool, 
                    x => (availability: x.Availability,
                        date: x.Availability == PoolAvailabilityState.Available ? (DateTime?) null : now));
            m_Storage.SavePoolAvailabilities(results);
        }
    }
}
