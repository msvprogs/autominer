using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.ControlCenterService.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Logic.Monitors
{
    public class PoolInfoMonitor : MonitorBase
    {
        private const int ParallelismDegree = 3;
        private static readonly PoolApiProtocol[] M_MultiPoolProtocols = {PoolApiProtocol.Yiimp};
        private static readonly TimeSpan M_LastOperationsPeriod = TimeSpan.FromDays(7);

        private readonly IPoolInfoProviderFactory m_ProviderFactory;
        private readonly Func<IPoolInfoMonitorStorage> m_StorageGetter;

        public PoolInfoMonitor(IPoolInfoProviderFactory providerFactory, Func<IPoolInfoMonitorStorage> storageGetter) 
            : base(TimeSpan.FromMinutes(10))
        {
            m_ProviderFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
            m_StorageGetter = storageGetter ?? throw new ArgumentNullException(nameof(storageGetter));
        }

        protected override void DoWork()
        {
            var pools = m_StorageGetter.Invoke().GetActivePools();

            var now = DateTime.UtcNow;
            var startDate = now - M_LastOperationsPeriod;
            var poolInfos = pools.Where(x => M_MultiPoolProtocols.Contains(x.ApiProtocol) && x.ApiUrl != null)
                .GroupBy(x => new {x.ApiProtocol, ApiUrl = x.ApiUrl.ToLowerInvariant().Trim()})
                .Select(x => new
                {
                    MultiKey = x.Key,
                    Provider = m_ProviderFactory.CreateMulti(x.Key.ApiProtocol, x.Key.ApiUrl, x.ToArray())
                })
                .SelectMany(x =>
                {
                    try
                    {
                        return x.Provider.GetInfo(startDate);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex,
                            $"Couldn't get data from pools with protocol {x.MultiKey.ApiProtocol}, URL {x.MultiKey.ApiUrl}");
                        return new Dictionary<Pool, PoolInfo>();
                    }
                })
                .Select(x => (pool:x.Key, info:x.Value))
                .Concat(pools.Where(x => x.ApiProtocol != PoolApiProtocol.None && !M_MultiPoolProtocols.Contains(x.ApiProtocol))
                    .Select(x => (pool:x, provider:m_ProviderFactory.Create(x)))
                    .AsParallel()
                    .WithDegreeOfParallelism(ParallelismDegree)
                    .Select(x =>
                    {
                        try
                        {
                            return (pool:x.pool, info: x.provider.GetInfo(startDate));
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Couldn't get data from pool {x.pool.Name}");
                            return (pool:null, info: null);
                        }
                    })
                    .Where(x => x.info != null))
                .Do(x => Log.Info($"Pool {x.pool.Name}: Balance {x.info.AccountInfo.ConfirmedBalance:N6} {x.pool.Coin.Symbol}, "
                                  + $"Unconfirmed {x.info.AccountInfo.UnconfirmedBalance:N6} {x.pool.Coin.Symbol}, "
                                  + $"Hashrate {ConversionHelper.ToHashRateWithUnits(x.info.AccountInfo.HashRate, x.pool.Coin.Algorithm.KnownValue)},"
                                  + $" Shares V:{x.info.AccountInfo.ValidShares}, I:{x.info.AccountInfo.InvalidShares}, "
                                  + $"Total Workers {x.info.State.TotalWorkers}, "
                                  + $"Total Hashrate {ConversionHelper.ToHashRateWithUnits(x.info.State.TotalHashRate, x.pool.Coin.Algorithm.KnownValue)}"))
                .ToArray();

            m_StorageGetter.Invoke().StorePoolAccountStates(poolInfos
                .Select(x => new PoolAccountState
                {
                    DateTime = now,
                    ConfirmedBalance = x.info.AccountInfo.ConfirmedBalance,
                    HashRate = x.info.AccountInfo.HashRate,
                    InvalidShares = x.info.AccountInfo.InvalidShares,
                    ValidShares = x.info.AccountInfo.ValidShares,
                    PoolHashRate = x.info.State.TotalHashRate,
                    PoolId = x.pool.Id,
                    PoolLastBlock = x.info.State.LastBlock,
                    PoolWorkers = x.info.State.TotalWorkers,
                    UnconfirmedBalance = x.info.AccountInfo.UnconfirmedBalance
                })
                .ToArray());

            var newPayments = poolInfos
                .SelectMany(x => x.info.PaymentsData
                    .EmptyIfNull()
                    .Select(y => new PoolPayment
                    {
                        ExternalId = y.ExternalId ?? y.DateTime.Ticks.ToString(),
                        Amount = y.Amount,
                        DateTime = y.DateTime,
                        PoolId = x.pool.Id,
                        Transaction = y.Transaction
                    })
                    .Where(y => y.DateTime >= startDate))
                .ToArray();
            var existingPayments = m_StorageGetter.Invoke().LoadExistingPayments(
                newPayments.Select(x => x.ExternalId).ToArray(), startDate);
            m_StorageGetter.Invoke().StorePoolPayments(newPayments
                .Except(existingPayments, new PoolPaymentEqualityComparer())
                .ToArray());
        }

        private class PoolPaymentEqualityComparer : EqualityComparer<PoolPayment>
        {
            public override bool Equals(PoolPayment x, PoolPayment y)
                => x.ExternalId == y.ExternalId && x.PoolId == y.PoolId;

            public override int GetHashCode(PoolPayment obj)
                => obj.ExternalId.GetHashCode() ^ obj.PoolId.GetHashCode();
        }
    }
}
