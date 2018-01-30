using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.MarketInfoProviders
{
    //Reddit discussion: https://www.reddit.com/r/CryptoBridge/comments/7sv3c8/developer_api_anywhere/
    public class CryptoBridgeMarketInfoProvider : IMarketInfoProvider
    {
        private static readonly Uri M_BaseUri = new Uri("https://api.crypto-bridge.org/api/v1/");

        public bool HasMarketsCountLimit => false;

        private readonly IWebClient m_WebClient;

        public CryptoBridgeMarketInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public ExchangeCurrencyInfo[] GetCurrencies()
            => new ExchangeCurrencyInfo[0];

        public CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos)
            => JsonConvert.DeserializeObject<JArray>(
                    m_WebClient.DownloadString(new Uri(M_BaseUri, "ticker")))
                .Cast<dynamic>()
                .Select(x => new
                {
                    PairSymbols = ((string) x.id).Split('_'),
                    Data = x
                })
                .Where(x => x.PairSymbols.Length > 1)
                .Select(x => new CurrencyMarketInfo
                {
                    IsActive = true,
                    SourceSymbol = x.PairSymbols[0],
                    TargetSymbol = x.PairSymbols[1],
                    HighestBid = (double) x.Data.bid,
                    //x.Data.volume is BTC volume
                    LastDayVolume = (double) x.Data.volume / (double) x.Data.last,
                    LowestAsk = (double) x.Data.ask,
                    LastPrice = (double) x.Data.last
                })
                .ToArray();
    }
}
