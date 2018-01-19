using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Logic.Storage
{
    public class PoolAvailabilityMonitorStorage : IPoolAvailabilityMonitorStorage
    {
        private readonly string m_ConnectionString;

        public PoolAvailabilityMonitorStorage(string connectionString)
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
                    .Where(x => x.Activity == ActivityState.Active)
                    .Where(x => x.Coin.Activity == ActivityState.Active)
                    .ToArray();
        }

        public void SavePoolResponseStoppedDates(Dictionary<int, DateTime?> dates)
        {
            if (dates == null)
                throw new ArgumentNullException(nameof(dates));

            using (var context = new AutoMinerDbContext(m_ConnectionString))
            {
                var poolIds = dates.Keys.ToArray();
                context.Pools
                    .Where(x => poolIds.Contains(x.Id))
                    .AsEnumerable()
                    .Join(dates, x => x.Id, x => x.Key, (x, y) => (pool:x, date:y.Value))
                    .ForEach(x => x.pool.ResponsesStoppedDate = x.date);
                context.SaveChanges();
            }
        }
    }
}
