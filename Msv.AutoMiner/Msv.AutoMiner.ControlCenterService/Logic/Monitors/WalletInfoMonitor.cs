using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.ControlCenterService.External.Contracts;
using Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Logic.Monitors
{
    public class WalletInfoMonitor : MonitorBase
    {
        private static readonly TimeSpan M_LastOperationsPeriod = TimeSpan.FromDays(7);

        private readonly IWalletInfoProviderFactory m_ProviderFactory;
        private readonly Func<IWalletInfoMonitorStorage> m_StorageGetter;

        public WalletInfoMonitor(IWalletInfoProviderFactory providerFactory, Func<IWalletInfoMonitorStorage> storageGetter) 
            : base(TimeSpan.FromMinutes(30))
        {
            m_ProviderFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
            m_StorageGetter = storageGetter ?? throw new ArgumentNullException(nameof(storageGetter));
        }

        protected override void DoWork()
        {
            var wallets = m_StorageGetter.Invoke().GetActiveWallets();

            var now = DateTime.UtcNow;
            var startDate = now - M_LastOperationsPeriod;

            var exchangeResults = wallets.Select(x => x.ExchangeType)
                .Where(x => x != null && x != ExchangeType.Unknown)
                .Select(x => x.Value)
                .Distinct()
                .Select(x => (exchange:x, provider: m_ProviderFactory.CreateExchange(x)))
                .Select(x =>
                {
                    try
                    {
                        var result = (x.exchange, balances:x.provider.GetBalances(), operations:x.provider
                            .GetOperations(startDate));
                        Log.Info($"Got {result.balances.Length} balances and {result.operations.Length} operations for exchange {x.exchange}");
                        return result;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Couldn't get data from exchange {x.exchange}");
                        return (x.exchange, balances:null, operations:null);
                    }
                })
                .Where(x => x.balances != null)
                .Select(x => new
                {
                    Data = x,
                    WalletCandidates = wallets.Where(y => y.ExchangeType == x.exchange)
                        .OrderByDescending(y => y.Created)
                        .ToArray()
                })
                .Where(x => x.WalletCandidates.Any())
                .ToArray();

            var localResults = wallets
                .Where(x => x.ExchangeType == null)
                .Where(x => x.Coin.NodeHost != null && x.Coin.NodeLogin != null && x.Coin.NodePassword != null)
                .Select(x => (wallet:x, provider:m_ProviderFactory.CreateLocal(x.Coin)))
                .Select(x =>
                {
                    try
                    {
                        return (x.wallet, balances: x.provider.GetBalances(), operations: x.provider
                            .GetOperations(startDate));
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Couldn't get data from local wallet of coin {x.wallet.Coin.Name}");
                        return (x.wallet, balances: null, operations: null);
                    }
                })
                .Where(x => x.balances != null)
                .ToArray();

            var balances = exchangeResults
                .Select(x => new
                {
                    MappingByAddress = x.WalletCandidates.Join(
                            x.Data.balances, y => y.Address, y => y.Address, (y, z) => (wallet: y, result: z))
                        .ToArray(),
                    MappingByCurrency = x.WalletCandidates.Join(
                            x.Data.balances, y => y.Coin.Symbol, y => y.CurrencySymbol,
                            (y, z) => (wallet: y, result: z))
                        .ToArray()
                })
                .SelectMany(x => x.MappingByAddress.Any() ? x.MappingByAddress : x.MappingByCurrency)
                .Concat(localResults.Select(x => (x.wallet, result: x.balances.First())))
                .Select(x => new WalletBalance
                {
                    WalletId = x.wallet.Id,
                    Balance = x.result.Available,
                    BlockedBalance = x.result.Blocked,
                    DateTime = now,
                    UnconfirmedBalance = x.result.Unconfirmed
                })
                .ToArray();
            m_StorageGetter.Invoke().StoreWalletBalances(balances);

            var newOperations = exchangeResults
                .SelectMany(x => x.WalletCandidates.Join(
                    x.Data.operations, y => y.Coin.Symbol, y => y.CurrencySymbol, (y, z) => (wallet: y, operation: z)))
                .Concat(localResults.SelectMany(x => x.operations.Select(y => (x.wallet, operation: y))))
                .Where(x => x.operation.DateTime >= startDate)
                .Select(x => new WalletOperation
                {
                    DateTime = x.operation.DateTime,
                    ExternalId = x.operation.ExternalId ?? x.operation.DateTime.Ticks.ToString(),
                    Amount = x.operation.Amount,
                    TargetAddress = x.operation.Address,
                    Transaction = x.operation.Transaction,
                    WalletId = x.wallet.Id
                })
                .ToArray();
            var operationIds = newOperations.Select(x => x.ExternalId).ToArray();
            var existingOperations = m_StorageGetter.Invoke().LoadExistingOperations(operationIds, startDate);

            m_StorageGetter.Invoke().StoreWalletOperations(newOperations
                .Except(existingOperations, new WalletOperationEqualityComparer())
                .ToArray());
        }

        private class WalletOperationEqualityComparer : EqualityComparer<WalletOperation>
        {
            public override bool Equals(WalletOperation x, WalletOperation y)
                => x.ExternalId == y.ExternalId && x.WalletId == y.WalletId;

            public override int GetHashCode(WalletOperation obj)
                => obj.ExternalId.GetHashCode() ^ obj.WalletId.GetHashCode();
        }
    }
}
