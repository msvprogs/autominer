using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Models.CoinInfoService;

namespace Msv.AutoMiner.Data.Logic
{
    public class StoredFiatValueProvider : IStoredFiatValueProvider
    {
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

            var minDate = DateTime.UtcNow - TimeSpan.FromHours(6);
            var values = m_Context.CoinFiatValues
                .AsNoTracking()
                .FromSql(@"SELECT source.* FROM CoinFiatValues source
  JOIN (SELECT CoinId, FiatCurrencyId, Source, MAX(DateTime) AS MaxDateTime FROM CoinFiatValues
  GROUP BY CoinId, FiatCurrencyId, Source) AS grouped
  ON source.CoinId = grouped.CoinId
  AND source.FiatCurrencyId = grouped.FiatCurrencyId
  AND source.Source = grouped.Source
  AND source.DateTime = grouped.MaxDateTime")
                .Where(x => x.Coin.Symbol == currency && x.FiatCurrency.Symbol == fiatCurrency)
                .Where(x => x.DateTime > minDate)
                .AsEnumerable()
                .DefaultIfEmpty(new CoinFiatValue{DateTime = DateTime.UtcNow})
                .ToArray();
            return new TimestampedValue
            {
                DateTime = values.Max(x => x.DateTime),
                Value = values.Average(x => x.Value)
            };
        }

        public TimestampedValue GetLastBtcUsdValue()
            => GetLastFiatValue("BTC", "USD");
    }
}

