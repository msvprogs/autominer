using System;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<TimestampedValue> GetLastFiatValueAsync(string currency, string fiatCurrency)
        {
            if (currency == null)
                throw new ArgumentNullException(nameof(currency));
            if (fiatCurrency == null)
                throw new ArgumentNullException(nameof(fiatCurrency));

            var maxDate = await m_Context.CoinFiatValues
                .AsNoTracking()
                .Where(x => x.Coin.Symbol == currency && x.FiatCurrency.Symbol == fiatCurrency)
                .Select(x => x.DateTime)
                .DefaultIfEmpty(DateTime.UtcNow)
                .MaxAsync();

            var values = await m_Context.CoinFiatValues
                .AsNoTracking()
                .Where(x => x.Coin.Symbol == currency && x.FiatCurrency.Symbol == fiatCurrency && x.DateTime == maxDate)
                .Select(x => x.Value)
                .ToArrayAsync();
            return new TimestampedValue
            {
                DateTime = maxDate,
                Value = values.DefaultIfEmpty(0).Average()
            };
        }

        public Task<TimestampedValue> GetLastBtcUsdValueAsync()
            => GetLastFiatValueAsync("BTC", "USD");
    }
}

