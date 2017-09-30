using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Logic.Storage
{
    public class PoolInfoMonitorStorage : IPoolInfoMonitorStorage
    {
        private readonly string m_ConnectionString;

        public PoolInfoMonitorStorage(string connectionString)
        {
            m_ConnectionString = connectionString;
        }

        public Pool[] GetActivePools()
        {
            using (var context = new AutoMinerDbContext(m_ConnectionString))
                return context.Pools
                    .Include(x => x.Coin)
                    .Include(x => x.Coin.Algorithm)
                    .Include(x => x.Coin.Wallets)
                    .AsNoTracking()
                    .Where(x => x.Activity != ActivityState.Deleted)
                    .ToArray();
        }

        public void StorePoolAccountStates(PoolAccountState[] poolAccountStates)
        {
            if (poolAccountStates == null)
                throw new ArgumentNullException(nameof(poolAccountStates));

            using (var context = new AutoMinerDbContext(m_ConnectionString))
            {
                context.PoolAccountStates.AddRange(poolAccountStates);
                context.SaveChanges();
            }
        }

        public void StorePoolPayments(PoolPayment[] poolPayments)
        {
            if (poolPayments == null)
                throw new ArgumentNullException(nameof(poolPayments));

            using (var context = new AutoMinerDbContext(m_ConnectionString))
            {
                context.PoolPayments.AddRange(poolPayments);
                context.SaveChanges();
            }
        }

        public PoolPayment[] LoadExistingPayments(string[] externalIds, DateTime startDate)
        {
            if (externalIds == null)
                throw new ArgumentNullException(nameof(externalIds));

            using (var context = new AutoMinerDbContext(m_ConnectionString))
            {
                return context.PoolPayments
                    .AsNoTracking()
                    .Where(x => x.DateTime >= startDate && externalIds.Contains(x.ExternalId))
                    .ToArray();
            }
        }
    }
}
