using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.FrontEnd.Providers
{
    public class WalletBalanceProvider : IWalletBalanceProvider
    {
        private static readonly TimeSpan M_MinDatePeriod = TimeSpan.FromDays(4);
        private readonly AutoMinerDbContext m_Context;

        public WalletBalanceProvider(AutoMinerDbContext context)
            => m_Context = context;

        public WalletBalance[] GetLastBalances()
        {
            var minDate = DateTime.UtcNow - M_MinDatePeriod;
            var maxDates = m_Context.WalletBalances
                .AsNoTracking()
                .Where(x => x.DateTime > minDate)
                .Select(x => new { x.WalletId, x.DateTime })
                .AsEnumerable()
                .GroupBy(x => x.WalletId)
                .Select(x => x.OrderByDescending(y => y.DateTime).First().DateTime)
                .Distinct()
                .ToArray();

            return m_Context.WalletBalances
                .AsNoTracking()
                .Where(x => maxDates.Contains(x.DateTime))
                .AsEnumerable()
                .GroupBy(x => x.WalletId)
                .Select(x => x.OrderByDescending(y => y.DateTime).First())
                .ToArray();
        }

        public Dictionary<ExchangeType, DateTime> GetLastBalanceDates()
        {
            var minDate = DateTime.UtcNow - M_MinDatePeriod;
            return m_Context.WalletBalances
                .AsNoTracking()
                .Include(x => x.Wallet)
                .Where(x => x.DateTime > minDate)
                .Where(x => x.Wallet.ExchangeType != null)
                .Select(x => new { Exchange = x.Wallet.ExchangeType.Value, x.DateTime })
                .AsEnumerable()
                .GroupBy(x => x.Exchange)
                .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.DateTime).First().DateTime);
        }
    }
}
