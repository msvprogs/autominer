using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Storage.Contracts;

namespace Msv.AutoMiner.Service.Storage
{
    public class PoolAccountMonitorStorage : IPoolAccountMonitorStorage
    {
        public Pool[] GetPools()
        {
            using (var context = new AutoMinerDbContext())
                return context.Pools
                    .Include(x => x.Coins)
                    .Where(x => x.Activity != ActivityState.Deleted)
                    .ToArray();
        }

        public Dictionary<int, DateTime> GetPoolLastPaymentDates()
        {
            using (var context = new AutoMinerDbContext())
                return context.PoolPayments
                    .Where(x => x.Pool.Activity != ActivityState.Deleted)
                    .GroupBy(x => x.PoolId)
                    .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.DateTime).First().DateTime);
        }

        public void SaveAccountStates(PoolAccountState[] states)
        {
            using (var context = new AutoMinerDbContext())
            {
                context.PoolAccountStates.AddRange(states);
                context.SaveChanges();
            }
        }

        public void SavePoolPayments(PoolPayment[] payments)
        {
            using (var context = new AutoMinerDbContext())
            {
                context.PoolPayments.AddRange(payments);
                context.SaveChanges();
            }
        }
    }
}
