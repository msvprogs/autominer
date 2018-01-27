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

        public void SavePoolAvailabilities(Dictionary<Pool, (PoolAvailabilityState availability, DateTime? date)> availabilities)
        {
            if (availabilities == null)
                throw new ArgumentNullException(nameof(availabilities));

            using (var context = m_Factory.Create())
            {
                var poolIds = availabilities.Keys.Select(x => x.Id).ToArray();
                context.Pools
                    .Where(x => poolIds.Contains(x.Id))
                    .AsEnumerable()
                    .Join(availabilities, x => x.Id, x => x.Key.Id, (x, y) => (pool:x, data:y.Value))
                    .ForEach(x =>
                    {
                        x.pool.Availability = x.data.availability;
                        x.pool.ResponsesStoppedDate = x.data.date;
                    });
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
