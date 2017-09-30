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
        private readonly string m_ConnectionString;

        public WalletInfoMonitorStorage(string connectionString)
        {
            m_ConnectionString = connectionString;
        }

        public Wallet[] GetActiveWallets()
        {
            using (var context = new AutoMinerDbContext(m_ConnectionString))
                return context.Wallets
                    .Include(x => x.Coin)
                    .AsNoTracking()
                    .Where(x => x.Activity != ActivityState.Deleted)
                    .ToArray();
        }

        public void StoreWalletBalances(WalletBalance[] balances)
        {
            if (balances == null)
                throw new ArgumentNullException(nameof(balances));

            using (var context = new AutoMinerDbContext(m_ConnectionString))
            {
                context.WalletBalances.AddRange(balances);
                context.SaveChanges();
            }
        }

        public WalletOperation[] LoadExistingOperations(string[] externalIds, DateTime startDate)
        {
            if (externalIds == null)
                throw new ArgumentNullException(nameof(externalIds));

            using (var context = new AutoMinerDbContext(m_ConnectionString))
                return context.WalletOperations
                    .AsNoTracking()
                    .Where(x => x.DateTime >= startDate && externalIds.Contains(x.ExternalId))
                    .ToArray();
        }

        public void StoreWalletOperations(WalletOperation[] operations)
        {
            if (operations == null)
                throw new ArgumentNullException(nameof(operations));

            using (var context = new AutoMinerDbContext(m_ConnectionString))
            {
                context.WalletOperations.AddRange(operations);
                context.SaveChanges();
            }
        }
    }
}