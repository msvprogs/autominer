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
        private const int ParallelismDegree = 6;

        private static readonly PoolApiProtocol[] M_MultiPoolProtocols = {PoolApiProtocol.Yiimp};
        private static readonly TimeSpan M_LastOperationsPeriod = TimeSpan.FromDays(7);

        private readonly IPoolInfoProviderFactory m_ProviderFactory;
        private readonly IPoolInfoMonitorStorage m_Storage;

        public PoolInfoMonitor(IPoolInfoProviderFactory providerFactory, IPoolInfoMonitorStorage storage) 
            : base(TimeSpan.FromMinutes(15))
        {
            m_ProviderFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
            m_Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        protected override void DoWork()
        {
            var pools = m_Storage.GetActivePools();
            var btcMiningTarget = m_Storage.GetBitCoinMiningTarget();

            var now = DateTime.UtcNow;
            var startDate = now - M_LastOperationsPeriod;

            var registeredMultiPools = pools
                .Where(x => M_MultiPoolProtocols.Contains(x.ApiProtocol) && x.ApiUrl != null)
                .GroupBy(x => new {x.ApiProtocol, ApiUrl = x.ApiUrl.ToLowerInvariant().Trim()});
            var multiPoolInfos = m_Storage.GetActiveMultiCoinPools()
                .Where(x => x.ApiUrl != null)
                .FullOuterJoin(registeredMultiPools,
                    x => new {x.ApiProtocol, ApiUrl = x.ApiUrl.ToLowerInvariant().Trim()},
                    x => x.Key,
                    (x, y) => new
                    {
                        MultiCoinPoolId = x?.Id,
                        ApiProtocol = x?.ApiProtocol ?? y.Key.ApiProtocol,
                        ApiUrl = x?.ApiUrl ?? y.Key.ApiUrl,
                        Provider = m_ProviderFactory.CreateMulti(
                            x?.ApiProtocol ?? y.Key.ApiProtocol,
                            x?.ApiUrl ?? y.Key.ApiUrl, 
                            y?.ToArray().EmptyIfNull(),
                            btcMiningTarget)
                    })
                .AsParallel()
                .WithDegreeOfParallelism(ParallelismDegree)
                .Select(x =>
                {
                    try
                    {
                        var multiPoolInfo = x.Provider.GetInfo(startDate);
                        Log.Info($"Multicoin pool {x.ApiUrl}: got {multiPoolInfo.CurrencyInfos.Length} active currencies");
                        return new {x.MultiCoinPoolId, MultiPoolInfo = multiPoolInfo};
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Couldn't get data from {x.ApiProtocol} pools, URL {x.ApiUrl}");
                        return new {MultiCoinPoolId = (int?)null, MultiPoolInfo = new MultiPoolInfo()};
                    }
                })
                .ToArray();
            m_Storage.StoreMultiCoinPoolCurrencies(multiPoolInfos
                    .Where(x => x.MultiCoinPoolId != null)
                    .SelectMany(x => x.MultiPoolInfo.CurrencyInfos.Select(y => new MultiCoinPoolCurrency
                    {
                        Algorithm = y.Algorithm,
                        Name = y.Name,
                        Hashrate = y.Hashrate,
                        MultiCoinPoolId = x.MultiCoinPoolId.Value,
                        Port = y.Port,
                        Symbol = y.Symbol,
                        Workers = y.Workers
                    }))
                    .ToArray());

            var poolInfos = multiPoolInfos
                .SelectMany(x => x.MultiPoolInfo.PoolInfos.EmptyIfNull())
                .Select(x => (pool:x.Key, info:x.Value))
                .Concat(pools.Where(x => x.ApiProtocol != PoolApiProtocol.None && !M_MultiPoolProtocols.Contains(x.ApiProtocol))
                    .Select(x => (pool:x, provider:m_ProviderFactory.Create(x, btcMiningTarget)))
                    .AsParallel()
                    .WithDegreeOfParallelism(ParallelismDegree)
                    .Select(x =>
                    {
                        try
                        {
                            return (x.pool, info: x.provider.GetInfo(startDate.AddHours(x.pool.TimeZoneCorrectionHours)));
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
                                  + $"Total Hashrate {ConversionHelper.ToHashRateWithUnits(x.info.State.TotalHashRate, x.pool.Coin.Algorithm.KnownValue)}, "
                                  + $"Fee {(x.info.State.PoolFee != null ? x.info.State.PoolFee.Value.ToString("F2") : "<unknown>")}%"))
                .ToArray();

            var updatedPools = poolInfos.Where(x => x.info.State.PoolFee != null)
                .Where(x => Math.Abs(x.pool.FeeRatio - x.info.State.PoolFee.Value) > 0.01)
                .Do(x => x.pool.FeeRatio = x.info.State.PoolFee.GetValueOrDefault())
                .Select(x => x.pool)
                .ToArray();
            m_Storage.SavePools(updatedPools);

            m_Storage.StorePoolAccountStates(poolInfos
                .Select(x => new PoolAccountState
                {
                    DateTime = now,
                    ConfirmedBalance = x.info.AccountInfo.ConfirmedBalance.ZeroIfNaN(),
                    HashRate = x.info.AccountInfo.HashRate,
                    InvalidShares = x.info.AccountInfo.InvalidShares,
                    ValidShares = x.info.AccountInfo.ValidShares,
                    PoolHashRate = x.info.State.TotalHashRate,
                    PoolId = x.pool.Id,
                    PoolLastBlock = x.info.State.LastBlock,
                    PoolWorkers = x.info.State.TotalWorkers,
                    UnconfirmedBalance = x.info.AccountInfo.UnconfirmedBalance.ZeroIfNaN()
                })
                .ToArray());

            var wallets = m_Storage.GetWalletIds(poolInfos
                .SelectMany(x => x.info.PaymentsData.EmptyIfNull().Select(y => y.Address))
                .Where(x => x != null).ToArray());
            var newPayments = poolInfos
                .SelectMany(x => x.info.PaymentsData
                    .EmptyIfNull()
                    .Select(y => new PoolPayment
                    {
                        ExternalId = y.ExternalId ?? y.DateTime.Ticks.ToString(),
                        Amount = y.Amount.ZeroIfNaN(),
                        DateTime = y.DateTime.AddHours(-x.pool.TimeZoneCorrectionHours),
                        PoolId = x.pool.Id,
                        Transaction = y.Transaction,
                        BlockHash = y.BlockHash,
                        CoinAddress = y.Address,
                        Type = y.Type,
                        WalletId = y.Address != null && wallets.ContainsKey(y.Address)
                            ? wallets[y.Address]
                            : (int?)null
                    })
                    .Where(y => y.DateTime >= startDate))
                .ToArray();
            var existingPayments = m_Storage.LoadExistingPayments(
                newPayments.Select(x => x.ExternalId).ToArray(), startDate);
            m_Storage.StorePoolPayments(newPayments
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
