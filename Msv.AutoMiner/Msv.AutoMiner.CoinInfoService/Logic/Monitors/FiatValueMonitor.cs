using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Logic.Monitors
{
    public class FiatValueMonitor : MonitorBase
    {
        private readonly IFiatValueProviderFactory m_ProviderFactory;
        private readonly Func<IFiatValueMonitorStorage> m_StorageGetter;

        public FiatValueMonitor(IFiatValueProviderFactory providerFactory, Func<IFiatValueMonitorStorage> storageGetter)
            : base(TimeSpan.FromMinutes(5))
        {
            m_ProviderFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
            m_StorageGetter = storageGetter ?? throw new ArgumentNullException(nameof(storageGetter));
        }

        protected override void DoWork()
        {
            var storage = m_StorageGetter.Invoke();
            var coins = storage.GetCoins()
                .ToLookup(x => x.Symbol);
            var fiatCurrencies = storage.GetFiatCurrencies()
                .ToDictionary(x => x.Symbol);
            var now = DateTime.UtcNow;
            var values = EnumHelper.GetValues<CoinFiatValueSource>()
                .Where(x => x != CoinFiatValueSource.Unknown)
                .Select(x => (type:x, provider:m_ProviderFactory.Create(x)))
                .Select(x =>
                {
                    try
                    {
                        return ProcessSource(x.type, x.provider);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Couldn't get fiat currency values from {x.type}");
                        return (x.type, values:new CurrencyFiatValue[0]);
                    }
                })
                .Where(x => x.values.Any())
                .SelectMany(x => x.values
                    .Join(coins, y => y.CurrencySymbol, y => y.Key, (y, z) => z.Select(a => (value:y, coinId:a.Id)))
                    .SelectMany(y => y)
                    .Join(fiatCurrencies, y => y.value.FiatCurrencySymbol, y => y.Key,
                        (y, z) => (y.value, y.coinId, fiatId: z.Value.Id))
                    .Select(y => new CoinFiatValue
                    {
                        CoinId = y.coinId,
                        FiatCurrencyId = y.fiatId,
                        Source = x.type,
                        Value = y.value.Value,
                        DateTime = now
                    })
                    .Where(y => y.Value > 0))
                .ToArray();
            storage.StoreValues(values);
        }

        private (CoinFiatValueSource type, CurrencyFiatValue[] values) ProcessSource(
            CoinFiatValueSource type, IFiatValueProvider provider)
        {
            Log.Info($"Starting fiat currency values receiving from {type}...");
            var values = provider.GetFiatValues();
            Log.Info($"Got {values.Length} fiat currency values from {type}");
            return (type, values);
        }
    }
}
