using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Logic.Storage
{
    public class NetworkInfoMonitorStorage : INetworkInfoMonitorStorage
    {
        private readonly AutoMinerDbContext m_Context;

        public NetworkInfoMonitorStorage(AutoMinerDbContext context)
            => m_Context = context;

        public Coin[] GetCoins()
        {
            return m_Context.Coins
                .Include(x => x.Algorithm)
                .Where(x => x.Activity != ActivityState.Deleted)
                .ToArray();
        }

        public CoinNetworkInfo[] GetLastNetworkInfos()
        {
            return m_Context.CoinNetworkInfos
                .Where(x => x.Coin.Activity != ActivityState.Deleted)
                .GroupBy(x => x.CoinId)
                .Select(x => x.OrderByDescending(y => y.Created).First())
                .ToArray();
        }

        public void StoreNetworkInfo(CoinNetworkInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            m_Context.CoinNetworkInfos.Add(info);
            m_Context.SaveChanges();
        }
    }
}