using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Models.CoinInfoService;
using Msv.AutoMiner.Data.Logic.Contracts;

namespace Msv.AutoMiner.Data.Logic
{
    public class StoredFiatValueProvider : IStoredFiatValueProvider
    {
        private readonly IAutoMinerDbContextFactory m_Factory;

        public StoredFiatValueProvider(IAutoMinerDbContextFactory factory)
            => m_Factory = factory;

        public TimestampedValue GetLastFiatValue(string currency, string fiatCurrency, DateTime? dateTime)
        {
            if (currency == null)
                throw new ArgumentNullException(nameof(currency));
            if (fiatCurrency == null)
                throw new ArgumentNullException(nameof(fiatCurrency));

            using (var context = m_Factory.CreateReadOnly())
            {
                var minDate = DateTime.UtcNow - TimeSpan.FromDays(1.1);
                var values = context.CoinFiatValues
                    .FromSql(@"SELECT source.* FROM CoinFiatValues source
  JOIN (SELECT CoinId, FiatCurrencyId, Source, MAX(DateTime) AS MaxDateTime FROM CoinFiatValues
  WHERE @p0 IS NULL OR DateTime < @p0
  GROUP BY CoinId, FiatCurrencyId, Source) AS grouped
  ON source.CoinId = grouped.CoinId
  AND source.FiatCurrencyId = grouped.FiatCurrencyId
  AND source.Source = grouped.Source
  AND source.DateTime = grouped.MaxDateTime", dateTime)
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
        }

        public TimestampedValue GetLastBtcUsdValue(DateTime? dateTime = null)
            => GetLastFiatValue("BTC", "USD", dateTime);
    }
}

