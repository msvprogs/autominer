using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Models.CoinInfoService;

namespace Msv.AutoMiner.Data.Logic
{
    public class StoredFiatValueProvider : IStoredFiatValueProvider
    {
        private static readonly TimeSpan M_MinDatePeriod = TimeSpan.FromDays(4);

        private readonly AutoMinerDbContext m_Context;

        public StoredFiatValueProvider(AutoMinerDbContext context)
        {
            m_Context = context;
        }

        public TimestampedValue GetLastFiatValue(string currency, string fiatCurrency)
        {
            if (currency == null)
                throw new ArgumentNullException(nameof(currency));
            if (fiatCurrency == null)
                throw new ArgumentNullException(nameof(fiatCurrency));

            var minDate = DateTime.UtcNow - M_MinDatePeriod;
            var maxDates = m_Context.CoinFiatValues
                .AsNoTracking()
                .Where(x => x.Coin.Symbol == currency && x.FiatCurrency.Symbol == fiatCurrency)
                .Where(x => x.DateTime > minDate)
                .Select(x => new {x.Source, x.DateTime})
                .AsEnumerable()
                .GroupBy(x => x.Source)
                .Select(x => x.OrderByDescending(y => y.DateTime).First().DateTime)
                .Distinct()
                .ToArray();

            var values = m_Context.CoinFiatValues
                .AsNoTracking()
                .Where(x => x.Coin.Symbol == currency && x.FiatCurrency.Symbol == fiatCurrency)
                .Where(x => maxDates.Contains(x.DateTime))
                .AsEnumerable()
                .GroupBy(x => x.Source)
                .Select(x => x.OrderByDescending(y => y.DateTime).First().Value)
                .ToArray();
            return new TimestampedValue
            {
                DateTime = maxDates.DefaultIfEmpty(DateTime.UtcNow).Max(),
                Value = values.DefaultIfEmpty(0).Average()
            };
        }

        public TimestampedValue GetLastBtcUsdValue()
            => GetLastFiatValue("BTC", "USD");
    }
}

