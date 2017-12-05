using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Logic.Monitors
{
    public class MarketInfoMonitor : MonitorBase
    {
        private const string BtcSymbol = "BTC";

        private readonly IMarketInfoProviderFactory m_ProviderFactory;
        private readonly Func<IMarketInfoMonitorStorage> m_StorageGetter;

        public MarketInfoMonitor(IMarketInfoProviderFactory providerFactory, Func<IMarketInfoMonitorStorage> storageGetter) 
            : base(TimeSpan.FromMinutes(5))
        {
            m_ProviderFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
            m_StorageGetter = storageGetter ?? throw new ArgumentNullException(nameof(storageGetter));
        }

        protected override void DoWork()
        {
            var storage = m_StorageGetter.Invoke();
            var coins = storage.GetCoins()
                .ToLookup(x => x.Symbol, x => x.Id);
            var coinSymbols = coins.Select(x => x.Key).ToArray();
            var now = DateTime.UtcNow;
            var downloadedExchangeData = EnumHelper.GetValues<ExchangeType>()
                .Join(storage.GetExchanges().Where(x => x.Activity == ActivityState.Active), x => x, x => x.Type, (x, y) => x)
                .Select(x => (type:x, provider: m_ProviderFactory.Create(x)))
                .Select(x =>
                {
                    try
                    {
                        return ProcessExchange(x.type, x.provider, coinSymbols);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Error while trying to get market values from {x.type}");
                        return (x.type, currencies: new ExchangeCurrencyInfo[0], marketInfos: new CurrencyMarketInfo[0]);
                    }
                })
                .Where(x => x.marketInfos.Any())
                .ToArray();
            var exchangeCurrenciesData = downloadedExchangeData
                .Select(x => new
                {
                    ExchangeCoins = x.currencies
                        .Join(coins, y => y.Symbol, y => y.Key, (y, z) => z.Select(a => (coin:y, coinId:a)))
                        .SelectMany(y => y)
                        .Select(y => new ExchangeCoin
                        {
                            IsActive = y.coin.IsActive,
                            CoinId = y.coinId,
                            Exchange = x.type,
                            DateTime = now,
                            MinWithdrawAmount = y.coin.MinWithdrawAmount,
                            WithdrawalFee = y.coin.WithdrawalFee
                        }),
                    ExchangeMarketPrices = x.marketInfos
                        .Where(y => y.TargetSymbol == BtcSymbol)
                        .Join(coins, y => y.SourceSymbol, y => y.Key, (y, z) => z.Select(a => (coin: y, coinId: a)))
                        .SelectMany(y => y)
                        .Select(y => new ExchangeMarketPrice
                        {
                            ExchangeType = x.type,
                            IsActive = y.coin.IsActive,
                            LowestAsk = y.coin.LowestAsk,
                            HighestBid = y.coin.HighestBid,
                            LastDayLow = y.coin.LastDayLow,
                            LastDayHigh = y.coin.LastDayHigh,
                            LastDayVolume = y.coin.LastDayVolume,
                            LastPrice = y.coin.LastPrice,
                            SourceCoinId = y.coinId,
                            TargetCoinId = coins[y.coin.TargetSymbol].First(),
                            DateTime = now,
                            SellFeePercent = y.coin.SellFeePercent,
                            BuyFeePercent = y.coin.BuyFeePercent
                        })
                })
                .ToArray();
            storage.StoreExchangeCoins(exchangeCurrenciesData.SelectMany(x => x.ExchangeCoins).ToArray());
            storage.StoreMarketPrices(exchangeCurrenciesData.SelectMany(x => x.ExchangeMarketPrices).ToArray());
        }

        private (ExchangeType type, ExchangeCurrencyInfo[] currencies, CurrencyMarketInfo[] marketInfos) ProcessExchange(
            ExchangeType type, IMarketInfoProvider provider, string[] coinSymbols)
        {
            Log.Info($"Starting market prices request from {type}...");
            var currencies = provider.GetCurrencies();
            Log.Info($"{type} supports {currencies.Length} currencies");
            if (provider.HasMarketsCountLimit)
                Log.Info($"{type} has market price request limit; requesting only registered coins");
            var marketPrices = provider.GetCurrencyMarkets(currencies
                .Where(x => !provider.HasMarketsCountLimit || coinSymbols.Contains(x.Symbol))
                .ToArray());
            Log.Info($"Got {marketPrices.Length} market prices from {type}");
            return (type, currencies, marketPrices);
        }
    }
}
