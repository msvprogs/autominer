using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.MarketInfoProviders
{
    //API: https://bitzure.com/documents/api_v2
    public class BitzureMarketInfoProvider : IMarketInfoProvider
    {
        private const double ConversionFeePercent = 0.25;

        private static readonly Uri M_BaseUri = new Uri("https://bitzure.com");

        public bool HasMarketsCountLimit => false;

        private readonly IWebClient m_WebClient;

        public BitzureMarketInfoProvider(IWebClient webClient) 
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public ExchangeCurrencyInfo[] GetCurrencies() 
            => JsonConvert.DeserializeObject<JArray>(
                    m_WebClient.DownloadString(new Uri(M_BaseUri, "/api/v2/markets.json")))
                .Cast<dynamic>()
                .Select(x => new ExchangeCurrencyInfo
                {
                    Name = (string) x.id,
                    Symbol = ((string) x.name).Split('/')[0],
                    ExternalId = (string)x.name,
                    IsActive = true
                })
                .ToArray();

        public CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos)
        {
            if (currencyInfos == null)
                throw new ArgumentNullException(nameof(currencyInfos));

            return JsonConvert.DeserializeObject<JObject>(
                    m_WebClient.DownloadString(new Uri(M_BaseUri, "/api/v2/tickers.json")))
                .Properties()
                .Cast<dynamic>()
                .Join(currencyInfos, x => (string)x.Name, x => x.Name, (x, y) => new
                {
                    PairSymbols = y.ExternalId.Split('/'),
                    Data = x.Value.ticker
                })
                .Where(x => x.PairSymbols.Length > 1)
                .Select(x => new CurrencyMarketInfo
                {
                    SourceSymbol = x.PairSymbols[0],
                    TargetSymbol = x.PairSymbols[1],
                    HighestBid = (double)x.Data.buy,
                    LowestAsk = (double)x.Data.sell,
                    LastDayLow = (double)x.Data.low,
                    LastDayHigh = (double)x.Data.high,
                    LastDayVolume = (double)x.Data.vol,
                    LastPrice = (double)x.Data.last,
                    IsActive = true,
                    SellFeePercent = ConversionFeePercent,
                    BuyFeePercent = ConversionFeePercent
                })
                .ToArray();
        }
    }
}
