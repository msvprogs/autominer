using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.FrontEnd.Providers
{
    public class CoinNetworkInfoProvider : ICoinNetworkInfoProvider
    {
        private static readonly TimeSpan M_MinDatePeriod = TimeSpan.FromDays(4);
        private readonly AutoMinerDbContext m_Context;

        public CoinNetworkInfoProvider(AutoMinerDbContext context)
            => m_Context = context;

        public CoinNetworkInfo[] GetCurrentNetworkInfos()
        {
            var minDate = DateTime.UtcNow - M_MinDatePeriod;
            var maxDates = m_Context.CoinNetworkInfos
                .AsNoTracking()
                .Where(x => x.Created > minDate)
                .Select(x => new { x.CoinId, x.Created })
                .AsEnumerable()
                .GroupBy(x => x.CoinId)
                .Select(x => x.OrderByDescending(y => y.Created).First().Created)
                .Distinct()
                .ToArray();

            return m_Context.CoinNetworkInfos
                .AsNoTracking()
                .Where(x => maxDates.Contains(x.Created))
                .AsEnumerable()
                .GroupBy(x => x.CoinId)
                .Select(x => x.OrderByDescending(y => y.Created).First())
                .ToArray();
        }
    }
}
