using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;

namespace Msv.AutoMiner.ControlCenterService.Logic.Storage
{
    public class PoolInfoMonitorStorage : IPoolInfoMonitorStorage
    {
        private readonly IAutoMinerDbContextFactory m_Factory;

        public PoolInfoMonitorStorage(IAutoMinerDbContextFactory factory)
            => m_Factory = factory;

        public Pool[] GetActivePools()
        {
            using (var context = m_Factory.Create())
                return context.Pools
                    .Include(x => x.Coin)
                    .Include(x => x.Coin.Algorithm)
                    .Include(x => x.Coin.Wallets)
                    .AsNoTracking()
                    .Where(x => x.Activity != ActivityState.Deleted)
                    .Where(x => x.Coin.Activity != ActivityState.Deleted)
                    .ToArray();
        }

        public void StorePoolAccountStates(PoolAccountState[] poolAccountStates)
        {
            if (poolAccountStates == null)
                throw new ArgumentNullException(nameof(poolAccountStates));

            using (var context = m_Factory.Create())
            {
                context.PoolAccountStates.AddRange(poolAccountStates);
                context.SaveChanges();
            }
        }

        public void StorePoolPayments(PoolPayment[] poolPayments)
        {
            if (poolPayments == null)
                throw new ArgumentNullException(nameof(poolPayments));

            using (var context = m_Factory.Create())
            {
                context.PoolPayments.AddRange(poolPayments);
                context.SaveChanges();
            }
        }

        public void SavePools(Pool[] pools)
        {
            if (pools == null)
                throw new ArgumentNullException(nameof(pools));

            using (var context = m_Factory.Create())
            {
                var poolIds = pools.Select(x => x.Id).ToArray();
                var existingPools = context.Pools
                    .Where(x => poolIds.Contains(x.Id))
                    .ToArray();
                existingPools.Join(pools, x => x.Id, x => x.Id, (x, y) => (oldPool:x, newPool: y))
                    .ForEach(x => x.oldPool.FeeRatio = x.newPool.FeeRatio);
                context.SaveChanges();
            }
        }

        public PoolPayment[] LoadExistingPayments(string[] externalIds, DateTime startDate)
        {
            if (externalIds == null)
                throw new ArgumentNullException(nameof(externalIds));

            using (var context = m_Factory.Create())
            {
                return context.PoolPayments
                    .AsNoTracking()
                    .Where(x => x.DateTime >= startDate && externalIds.Contains(x.ExternalId))
                    .ToArray();
            }
        }

        public Dictionary<string, int> GetWalletIds(string[] addresses)
        {
            if (addresses == null)
                throw new ArgumentNullException(nameof(addresses));

            using (var context = m_Factory.Create())
            {
                return context.Wallets
                    .AsNoTracking()
                    .Where(x => addresses.Contains(x.Address))
                    .Select(x => new { x.Address, x.Id })
                    .AsEnumerable()
                    .GroupBy(x => x.Address)
                    .ToDictionary(x => x.Key, x => x.First().Id);
            }
        }

        public Wallet GetBitCoinMiningTarget()
        {
            using (var context = m_Factory.Create())
                return context.Wallets.FirstOrDefault(x => x.IsMiningTarget && x.Coin.Symbol == "BTC");
        }
    }
}
