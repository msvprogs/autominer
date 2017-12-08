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
        private static readonly TimeSpan M_MinDatePeriod = TimeSpan.FromDays(4);

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
            var minDate = DateTime.UtcNow - M_MinDatePeriod;
            var coinIds = m_Context.Coins
                .AsNoTracking()
                .Where(x => x.Activity != ActivityState.Deleted)
                .Select(x => x.Id)
                .ToArray();
            var maxDates = m_Context.CoinNetworkInfos
                .AsNoTracking()
                .Where(x => x.Created > minDate)
                .Where(x => coinIds.Contains(x.CoinId))
                .Select(x => new { x.CoinId, x.Created })
                .AsEnumerable()
                .GroupBy(x => x.CoinId)
                .Select(x => x.OrderByDescending(y => y.Created).First().Created)
                .Distinct()
                .ToArray();

            return m_Context.CoinNetworkInfos
                .AsNoTracking()
                .Include(x => x.Coin)
                .Include(x => x.Coin.Algorithm)
                .AsNoTracking()
                .Where(x => coinIds.Contains(x.CoinId) && maxDates.Contains(x.Created))
                .AsEnumerable()
                .GroupBy(x => x.CoinId)
                .Select(x => x.OrderByDescending(y => y.Created).First())
                .ToArray();
        }

        public void StoreNetworkInfo(CoinNetworkInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            lock (m_Context)
            {
                m_Context.CoinNetworkInfos.Add(info);
                m_Context.SaveChanges();
            }
        }
    }
}