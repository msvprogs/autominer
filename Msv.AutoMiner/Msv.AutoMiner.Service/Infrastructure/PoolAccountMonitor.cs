using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Msv.AutoMiner.Service.Infrastructure.Contracts;
using Msv.AutoMiner.Service.Storage.Contracts;
using NLog;
using Msv.AutoMiner.Commons;

namespace Msv.AutoMiner.Service.Infrastructure
{
    public class PoolAccountMonitor : IDisposable
    {
        private static readonly TimeSpan M_MonitorInterval = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan M_AllPoolsMonitorInterval = TimeSpan.FromMinutes(30);
        private static readonly ILogger M_Logger = LogManager.GetLogger("PoolAccountMonitor");

        private readonly IAutomaticMinerChanger m_MinerChanger;
        private readonly IPoolInfoProviderFactory m_ProviderFactory;
        private readonly IDDoSTriggerPreventingDownloader m_Downloader;
        private readonly IPoolAccountMonitorStorage m_Storage;
        private readonly IDisposable m_Subscription;

        private DateTime m_LastFullMonitoring;

        public PoolAccountMonitor(
            IAutomaticMinerChanger minerChanger, 
            IPoolInfoProviderFactory providerFactory,
            IDDoSTriggerPreventingDownloader downloader, 
            IPoolAccountMonitorStorage storage)
        {
            if (minerChanger == null)
                throw new ArgumentNullException(nameof(minerChanger));
            if (providerFactory == null)
                throw new ArgumentNullException(nameof(providerFactory));
            if (downloader == null)
                throw new ArgumentNullException(nameof(downloader));
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));

            m_MinerChanger = minerChanger;
            m_ProviderFactory = providerFactory;
            m_Downloader = downloader;
            m_Storage = storage;
            m_Subscription = Observable.Timer(M_MonitorInterval)
                .Repeat()
                .StartWith(Scheduler.Default, 0)
                .Subscribe(x =>
                {
                    try
                    {
                        DoWork();
                    }
                    catch (Exception ex)
                    {
                        M_Logger.Error(ex, "Pool account monitor exception");
                    }
                });
        }

        private void DoWork()
        {
            M_Logger.Debug("Started pool info updating...");
            var random = new Random();
            var doFullMonitoring = DateTime.Now - m_LastFullMonitoring >= M_AllPoolsMonitorInterval;
            var lastDates = m_Storage.GetPoolLastPaymentDates();
            var payments = new List<PoolPayment>();
            var states = m_Storage.GetPools()
                .Where(x => x.ApiProtocol != PoolApiProtocol.None
                            && x.Coins.Any(y => y.PoolId != null))
                .Where(x => doFullMonitoring
                            || m_MinerChanger.CurrentCoins != null
                            && x.Coins.Select(y => new { Symbol = y.CurrencySymbol, y.Algorithm })
                                .Intersect(m_MinerChanger.CurrentCoins
                                    .Select(y => new {y.Symbol, y.Algorithm}))
                                .Any())
                .OrderBy(x => random.NextDouble())
                .Select(x => new
                {
                    Pool = x,
                    CoinAlgorithm = x.Coins.First().Algorithm,
                    Provider = m_ProviderFactory.Create(x.Coins.First(), m_Downloader)
                })
                .Select(x =>
                {
                    try
                    {
                        var info = x.Provider.GetInfo(lastDates.TryGetValue(x.Pool.Id));
                        payments.AddRange(info.PaymentsData
                            .Select(y => new PoolPayment
                            {
                                Amount = y.Amount,
                                PoolId = x.Pool.Id,
                                DateTime = y.DateTime,
                                Transaction = y.Transaction
                            }));
                        M_Logger.Info($"Pool {x.Pool.Name}: Balance {info.AccountInfo.ConfirmedBalance:N6}, "
                                      + $"Unconfirmed {info.AccountInfo.UnconfirmedBalance:N6}, "
                                      + $"Hashrate {ConversionHelper.ToHashRateWithUnits(info.AccountInfo.Hashrate, x.CoinAlgorithm)},"
                                      + $" Shares V:{info.AccountInfo.ValidShares}, I:{info.AccountInfo.InvalidShares}, "
                                      + $"Total Workers {info.State.TotalWorkers}, "
                                      + $"Total Hashrate {ConversionHelper.ToHashRateWithUnits(info.State.TotalHashRate, x.CoinAlgorithm)}");
                        return new
                        {
                            x.Pool,
                            Info = info,
                        };
                    }
                    catch (Exception ex)
                    {
                        M_Logger.Error(ex, $"Couldn't get info for pool {x.Pool.Name}");
                        return new {x.Pool, Info = (PoolInfo) null};
                    }
                })
                .Where(x => x.Info != null)
                .Select(x => new PoolAccountState
                {
                    PoolId = x.Pool.Id,
                    DateTime = DateTime.Now,
                    ConfirmedBalance = x.Info.AccountInfo.ConfirmedBalance,
                    UnconfirmedBalance = x.Info.AccountInfo.UnconfirmedBalance,
                    HashRate = x.Info.AccountInfo.Hashrate,
                    ValidShares = x.Info.AccountInfo.ValidShares,
                    InvalidShares = x.Info.AccountInfo.InvalidShares,
                    PoolHashRate = x.Info.State.TotalHashRate,
                    PoolLastBlock = x.Info.State.LastBlock,
                    PoolWorkers = x.Info.State.TotalWorkers
                })
                .ToArray();
            if (doFullMonitoring)
                m_LastFullMonitoring = DateTime.Now;

            M_Logger.Debug("Pool info updating finished");
            if (!states.Any())
                return;
            m_Storage.SaveAccountStates(states);
            m_Storage.SavePoolPayments(payments
                .GroupBy(x => new {x.PoolId, x.DateTime})
                .Select(x => x.First())
                .ToArray());
        }

        public void Dispose() => m_Subscription.Dispose();
    }
}
