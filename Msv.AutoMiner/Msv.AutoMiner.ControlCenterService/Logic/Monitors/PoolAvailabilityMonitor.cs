using System;
using System.Linq;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Infrastructure;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts;
using Msv.AutoMiner.Data.Logic;

namespace Msv.AutoMiner.ControlCenterService.Logic.Monitors
{
    public class PoolAvailabilityMonitor : MonitorBase
    {        
        private const int ParallelismDegree = 4;

        private readonly IPoolAvailabilityChecker m_PoolAvailabilityChecker;
        private readonly Func<IPoolAvailabilityMonitorStorage> m_StorageGetter;

        public PoolAvailabilityMonitor(
            IPoolAvailabilityChecker poolAvailabilityChecker, 
            Func<IPoolAvailabilityMonitorStorage> storageGetter)
            : base(TimeSpan.FromMinutes(30))
        {
            m_PoolAvailabilityChecker = poolAvailabilityChecker ?? throw new ArgumentNullException(nameof(poolAvailabilityChecker));
            m_StorageGetter = storageGetter ?? throw new ArgumentNullException(nameof(storageGetter));
        }

        protected override void DoWork()
        {
            var now = DateTime.UtcNow;
            var btcMiningTarget = m_StorageGetter.Invoke().GetBitCoinMiningTarget();
            var results = m_StorageGetter.Invoke().GetActivePools()
                .AsParallel()
                .WithDegreeOfParallelism(ParallelismDegree)
                .Select(x => (pool:x, login: x.GetLogin(x.UseBtcWallet ? btcMiningTarget : x.Coin.Wallets.FirstOrDefault(y => y.IsMiningTarget))))
                .Where(x => x.login != null)
                .Select(x => new
                {
                    Pool = x.pool,
                    IsAvailable = m_PoolAvailabilityChecker.Check(new PoolDataModel
                    {
                        Login = x.login,
                        Password = x.pool.WorkerPassword,
                        Name = x.pool.Name,
                        Protocol = x.pool.Protocol,
                        Url = x.pool.GetUrl()
                    })
                })
                .Where(x => x.Pool.ResponsesStoppedDate == null ^ x.IsAvailable)
                .ToDictionary(x => x.Pool.Id, x => x.IsAvailable ? (DateTime?) null : now);
            m_StorageGetter.Invoke().SavePoolResponseStoppedDates(results);
        }
    }
}
