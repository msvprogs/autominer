using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Exchanges.Api;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.MarketInfoProviders
{
    public class GraviexMarketInfoProvider : IMarketInfoProvider
    {
        public bool HasMarketsCountLimit => false;
        public TimeSpan? RequestInterval => null;

        private readonly IExchangeApi m_ExchangeApi;

        public GraviexMarketInfoProvider(IExchangeApi exchangeApi)
            => m_ExchangeApi = exchangeApi ?? throw new ArgumentNullException(nameof(exchangeApi));

        public ExchangeCurrencyInfo[] GetCurrencies()
            => ((JArray)m_ExchangeApi.ExecutePublic("markets.json", new Dictionary<string, string>()))
                .Cast<dynamic>()
                .Select(x => new ExchangeCurrencyInfo
                {
                    IsActive = true,
                    ExternalId = x.id,
                    Symbol = ((string) x.name).Split('/')[0],
                    Name = (string) x.name
                })
                .ToArray();

        public CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos)
            => ((JObject)m_ExchangeApi.ExecutePublic("tickers.json", new Dictionary<string, string>()))
                .Properties()
                .Join(currencyInfos, x => x.Name, x => x.ExternalId,
                    (x, y) => (name: y.Name.Split('/', 2), ((dynamic) x.Value).ticker))
                .Where(x => x.name.Length == 2)
                .Select(x => new CurrencyMarketInfo
                {
                    IsActive = true,
                    HighestBid = (double) x.ticker.buy,
                    LowestAsk = (double) x.ticker.sell,
                    LastDayLow = (double) x.ticker.low,
                    LastDayHigh = (double) x.ticker.high,
                    LastPrice = (double) x.ticker.last,
                    LastDayVolume = (double) x.ticker.vol,
                    SourceSymbol = x.name[0],
                    TargetSymbol = x.name[1]
                })
                .ToArray();
    }
}
