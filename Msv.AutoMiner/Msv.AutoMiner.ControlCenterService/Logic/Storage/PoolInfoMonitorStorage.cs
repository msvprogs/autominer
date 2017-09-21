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
        private readonly AutoMinerDbContext m_Context;

        public PoolInfoMonitorStorage(AutoMinerDbContext context)
            => m_Context = context;

        public Pool[] GetActivePools()
            => m_Context.Pools
                .Include(x => x.Coin)
                .Include(x => x.Coin.Algorithm)
                .Include(x => x.Coin.Wallets)
                .AsNoTracking()
                .Where(x => x.Activity != ActivityState.Deleted)
                .ToArray();

        public void StorePoolAccountStates(PoolAccountState[] poolAccountStates)
        {
            if (poolAccountStates == null)
                throw new ArgumentNullException(nameof(poolAccountStates));

            m_Context.PoolAccountStates.AddRange(poolAccountStates);
            m_Context.SaveChanges();
        }

        public void StorePoolPayments(PoolPayment[] poolPayments)
        {
            if (poolPayments == null)
                throw new ArgumentNullException(nameof(poolPayments));

            m_Context.PoolPayments.AddRange(poolPayments);
            m_Context.SaveChanges();
        }

        public PoolPayment[] LoadExistingPayments(string[] externalIds, DateTime startDate)
        {
            if (externalIds == null)
                throw new ArgumentNullException(nameof(externalIds));

            return m_Context.PoolPayments
                .AsNoTracking()
                .Where(x => x.DateTime >= startDate && externalIds.Contains(x.ExternalId))
                .ToArray();
        }
    }
}
