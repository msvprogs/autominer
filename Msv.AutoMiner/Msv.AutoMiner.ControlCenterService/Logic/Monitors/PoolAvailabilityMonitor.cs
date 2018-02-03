using System;
using System.Linq;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.Infrastructure;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.ControlCenterService.Logic.Notifiers;
using Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts;
using Msv.AutoMiner.Data.Logic;

namespace Msv.AutoMiner.ControlCenterService.Logic.Monitors
{
    public class PoolAvailabilityMonitor : MonitorBase
    {        
        private const int ParallelismDegree = 6;

        private readonly IPoolAvailabilityChecker m_PoolAvailabilityChecker;
        private readonly INotifier m_Notifier;
        private readonly IPoolAvailabilityMonitorStorage m_Storage;

        public PoolAvailabilityMonitor(
            IPoolAvailabilityChecker poolAvailabilityChecker,
            INotifier notifier,
            IPoolAvailabilityMonitorStorage storage)
            : base(TimeSpan.FromMinutes(30))
        {
            m_PoolAvailabilityChecker = poolAvailabilityChecker ?? throw new ArgumentNullException(nameof(poolAvailabilityChecker));
            m_Notifier = notifier;
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
                    x => (availability: x.Availability == PoolAvailabilityState.NoResponse 
                                       && x.Pool.Availability == PoolAvailabilityState.Available 
                            ? PoolAvailabilityState.NoResponseAfterFirstAttempt
                            : x.Availability,
                        date: x.Availability == PoolAvailabilityState.Available ? (DateTime?) null : now));
            results.ForEach(x =>
            {
                switch (x.Value.availability)
                {
                    case PoolAvailabilityState.Available:
                        // do not notify if connection restored after first attempt
                        if (x.Key.Availability != PoolAvailabilityState.NoResponseAfterFirstAttempt)
                            m_Notifier.SendMessage($"Pool {x.Key.Name} is now responding normally");
                        break;
                    case PoolAvailabilityState.AuthenticationFailed:
                        m_Notifier.SendMessage(
                            $"Warning: Authentication on the pool {x.Key.Name} has failed. Please check your credentials, wallet address and the pool site.");
                        break;
                    case PoolAvailabilityState.NoResponse:
                        m_Notifier.SendMessage(
                            $"Warning: Pool {x.Key.Name} has stopped responding. Please check that it's still alive.");
                        break;
                    case PoolAvailabilityState.NoResponseAfterFirstAttempt:
                        break; //do not notify if connection failed after first attempt
                }
            });
            m_Storage.SavePoolAvailabilities(results);
        }
    }
}
