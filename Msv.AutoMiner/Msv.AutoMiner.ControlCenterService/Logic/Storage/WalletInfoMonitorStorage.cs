using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Logic.Storage
{
    public class WalletInfoMonitorStorage : IWalletInfoMonitorStorage
    {
        private readonly AutoMinerDbContext m_Context;

        public WalletInfoMonitorStorage(AutoMinerDbContext context)
            => m_Context = context;

        public Wallet[] GetActiveWallets()
            => m_Context.Wallets
                .Include(x => x.Coin)
                .AsNoTracking()
                .Where(x => x.Activity != ActivityState.Deleted)
                .ToArray();

        public void StoreWalletBalances(WalletBalance[] balances)
        {
            if (balances == null)
                throw new ArgumentNullException(nameof(balances));

            m_Context.WalletBalances.AddRange(balances);
            m_Context.SaveChanges();
        }

        public WalletOperation[] LoadExistingOperations(string[] externalIds, DateTime startDate)
        {
            if (externalIds == null)
                throw new ArgumentNullException(nameof(externalIds));

            return m_Context.WalletOperations
                .AsNoTracking()
                .Where(x => x.DateTime >= startDate && externalIds.Contains(x.ExternalId))
                .ToArray();
        }

        public void StoreWalletOperations(WalletOperation[] operations)
        {
            if (operations == null)
                throw new ArgumentNullException(nameof(operations));

            m_Context.WalletOperations.AddRange(operations);
            m_Context.SaveChanges();
        }
    }
}