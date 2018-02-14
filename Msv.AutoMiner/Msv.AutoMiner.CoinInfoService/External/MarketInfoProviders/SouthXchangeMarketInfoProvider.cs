using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.MarketInfoProviders
{
    //API: https://www.southxchange.com/Home/Api
    public class SouthXchangeMarketInfoProvider : IMarketInfoProvider
    {
        private static readonly Uri M_BaseUri = new Uri("https://www.southxchange.com/api/");

        public bool HasMarketsCountLimit => false;

        private readonly IWebClient m_WebClient;

        public SouthXchangeMarketInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public ExchangeCurrencyInfo[] GetCurrencies()
            => JsonConvert.DeserializeObject<JArray>(m_WebClient.DownloadString(new Uri(M_BaseUri, "markets")))
                .Cast<JArray>()
                .Select(x => x[0].Value<string>())
                .Distinct()
                .Select(x => new ExchangeCurrencyInfo
                {
                    IsActive = true,
                    Symbol = x,
                    Name = x
                })
                .ToArray();

        public CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos)
            => JsonConvert.DeserializeObject<JArray>(m_WebClient.DownloadString(new Uri(M_BaseUri, "prices")))
                .Cast<dynamic>()
                .Select(x => new
                {
                    PairSymbols = ((string) x.Market).Split('/'),
                    Data = x
                })
                .Where(x => x.PairSymbols.Length > 1)
                .Select(x => new CurrencyMarketInfo
                {
                    IsActive = true,
                    HighestBid = (double?) x.Data.Bid ?? 0,
                    LastDayVolume = (double?) x.Data.Volume24Hr ?? 0,
                    LastPrice = (double?) x.Data.Last ?? 0,
                    LowestAsk = (double?) x.Data.Ask ?? 0,
                    SourceSymbol = x.PairSymbols[0],
                    TargetSymbol = x.PairSymbols[1]
                })
                .ToArray();
    }
}
