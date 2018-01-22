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
    public class PoolAvailabilityMonitorStorage : IPoolAvailabilityMonitorStorage
    {
        private readonly IAutoMinerDbContextFactory m_Factory;

        public PoolAvailabilityMonitorStorage(IAutoMinerDbContextFactory factory)
            => m_Factory = factory;

        public Pool[] GetActivePools()
        {
            using (var context = m_Factory.CreateReadOnly())
                return context.Pools
                    .Include(x => x.Coin)
                    .Include(x => x.Coin.Algorithm)
                    .Include(x => x.Coin.Wallets)
                    .Where(x => x.Activity == ActivityState.Active)
                    .Where(x => x.Coin.Activity == ActivityState.Active)
                    .ToArray();
        }

        public void SavePoolResponseStoppedDates(Dictionary<int, DateTime?> dates)
        {
            if (dates == null)
                throw new ArgumentNullException(nameof(dates));

            using (var context = m_Factory.Create())
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

        public Wallet GetBitCoinMiningTarget()
        {
            using (var context = m_Factory.CreateReadOnly())
                return context.Wallets.FirstOrDefault(x => x.IsMiningTarget && x.Coin.Symbol == "BTC");
        }
    }
}
