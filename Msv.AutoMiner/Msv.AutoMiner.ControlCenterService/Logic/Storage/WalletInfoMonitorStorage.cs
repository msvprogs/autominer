using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.Data.Logic.Contracts;

namespace Msv.AutoMiner.ControlCenterService.Logic.Storage
{
    public class WalletInfoMonitorStorage : IWalletInfoMonitorStorage
    {
        private readonly IAutoMinerDbContextFactory m_Factory;

        public WalletInfoMonitorStorage(IAutoMinerDbContextFactory factory)
            => m_Factory = factory;

        public Wallet[] GetActiveWallets()
        {
            using (var context = m_Factory.CreateReadOnly())
                return context.Wallets
                    .Include(x => x.Coin)
                    .Where(x => x.Activity == ActivityState.Active)
                    .Where(x => x.ExchangeType == null || x.Exchange.Activity == ActivityState.Active)
                    .Where(x => x.Coin.Activity != ActivityState.Deleted)
                    .ToArray();
        }

        public void StoreWalletBalances(WalletBalance[] balances)
        {
            if (balances == null)
                throw new ArgumentNullException(nameof(balances));

            using (var context = m_Factory.Create())
            {
                context.WalletBalances.AddRange(balances);
                context.SaveChanges();
            }
        }

        public WalletOperation[] LoadExistingOperations(string[] externalIds, DateTime startDate)
        {
            if (externalIds == null)
                throw new ArgumentNullException(nameof(externalIds));

            using (var context = m_Factory.CreateReadOnly())
                return context.WalletOperations
                    .Where(x => x.DateTime >= startDate && externalIds.Contains(x.ExternalId))
                    .ToArray();
        }

        public void StoreWalletOperations(WalletOperation[] operations)
        {
            if (operations == null)
                throw new ArgumentNullException(nameof(operations));

            using (var context = m_Factory.Create())
            {
                context.WalletOperations.AddRange(operations);
                context.SaveChanges();
            }
        }
    }
}