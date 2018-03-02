using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.MarketInfoProviders
{
    //API: https://graviex.net/documents/api_v2
    public class GraviexMarketInfoProvider : IMarketInfoProvider
    {
        private static readonly Uri M_BaseUri = new Uri("https://graviex.net/api/v2/");

        public bool HasMarketsCountLimit => false;
        public TimeSpan? RequestInterval => null;

        private readonly IWebClient m_WebClient;

        public GraviexMarketInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public ExchangeCurrencyInfo[] GetCurrencies()
            => JsonConvert.DeserializeObject<JArray>(m_WebClient.DownloadString(new Uri(M_BaseUri, "markets.json")))
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
            => JsonConvert.DeserializeObject<JObject>(m_WebClient.DownloadString(new Uri(M_BaseUri, "tickers.json")))
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
