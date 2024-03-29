﻿using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.ControlCenterService.External.Contracts;
using Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Logic.Monitors
{
    public class WalletInfoMonitor : MonitorBase
    {
        private static readonly TimeSpan M_LastOperationsPeriod = TimeSpan.FromDays(7);

        private readonly IWalletInfoProviderFactory m_ProviderFactory;
        private readonly IWalletInfoMonitorStorage m_Storage;

        public WalletInfoMonitor(IWalletInfoProviderFactory providerFactory, IWalletInfoMonitorStorage storage) 
            : base(TimeSpan.FromMinutes(30))
        {
            m_ProviderFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
            m_Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        protected override void DoWork()
        {
            var wallets = m_Storage.GetActiveWallets();
            var now = DateTime.UtcNow;
            var startDate = now - M_LastOperationsPeriod;

            var exchangeResults = wallets
                .Select(x => x.ExchangeType)
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
                .Select(x => (wallet:x, provider:m_ProviderFactory.CreateLocal(x.Coin, x.BalanceSource)))
                .Select(x =>
                {
                    try
                    {
                        var result = (x.wallet, balance: x.provider.GetBalance(x.wallet.Address), operations: x.provider
                            .GetOperations(x.wallet.Address, startDate));
                        Log.Info($"Got balance for local wallet {x.wallet.Address} ({x.wallet.Coin.Symbol})");
                        return result;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Couldn't get data from local wallet of coin {x.wallet.Coin.Name}");
                        return (x.wallet, balance: null, operations: null);
                    }
                })
                .Where(x => x.balance != null)
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
                .Concat(localResults.Select(x => (x.wallet, result: x.balance)))
                .Select(x => new WalletBalance
                {
                    WalletId = x.wallet.Id,
                    Balance = x.result.Available.ZeroIfNaN(),
                    BlockedBalance = x.result.Blocked.ZeroIfNaN(),
                    DateTime = now,
                    UnconfirmedBalance = x.result.Unconfirmed.ZeroIfNaN()
                })
                .ToArray();
            m_Storage.StoreWalletBalances(balances);

            var newOperations = exchangeResults
                .SelectMany(x => x.WalletCandidates.Join(
                    x.Data.operations, y => y.Coin.Symbol, y => y.CurrencySymbol, (y, z) => (wallet: y, operation: z)))
                .Concat(localResults.SelectMany(x => x.operations.Select(y => (x.wallet, operation: y))))
                .Where(x => x.operation.DateTime >= startDate)
                .Select(x => new WalletOperation
                {
                    DateTime = x.operation.DateTime,
                    ExternalId = x.operation.ExternalId ?? x.operation.DateTime.Ticks.ToString(),
                    Amount = x.operation.Amount.ZeroIfNaN(),
                    TargetAddress = x.operation.Address,
                    Transaction = x.operation.Transaction,
                    WalletId = x.wallet.Id
                })
                .ToArray();
            var operationIds = newOperations.Select(x => x.ExternalId).ToArray();
            var existingOperations = m_Storage.LoadExistingOperations(operationIds, startDate);

            m_Storage.StoreWalletOperations(newOperations
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
