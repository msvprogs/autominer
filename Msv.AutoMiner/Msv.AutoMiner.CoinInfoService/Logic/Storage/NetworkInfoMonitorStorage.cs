using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.Data.Logic.Contracts;

namespace Msv.AutoMiner.CoinInfoService.Logic.Storage
{
    public class NetworkInfoMonitorStorage : INetworkInfoMonitorStorage
    {
        private readonly IAutoMinerDbContextFactory m_Factory;

        public NetworkInfoMonitorStorage(IAutoMinerDbContextFactory factory)
            => m_Factory = factory;

        public Coin[] GetCoins()
        {
            using (var context = m_Factory.CreateReadOnly())
                return context.Coins
                    .Include(x => x.Algorithm)
                    .Where(x => x.Activity != ActivityState.Deleted)
                    .ToArray();
        }

        public void StoreNetworkInfo(CoinNetworkInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            using (var context = m_Factory.Create())
            {
                context.CoinNetworkInfos.Add(info);
                context.SaveChanges();
            }
        }
    }
}