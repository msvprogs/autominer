using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.MarketInfoProviders
{
    //API: https://btc-alpha.github.io/api-docs
    public class BtcAlphaMarketInfoProvider : IMarketInfoProvider
    {       
        private static readonly Uri M_BaseUri = new Uri("https://btc-alpha.com/api/");

        public bool HasMarketsCountLimit => true;

        private readonly IWebClient m_WebClient;

        public BtcAlphaMarketInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public ExchangeCurrencyInfo[] GetCurrencies() 
            => DoRequest("v1/currencies")
                .Cast<dynamic>()
                .Select(x => new ExchangeCurrencyInfo
                {
                    Name = (string) x.short_name,
                    Symbol = (string) x.short_name,
                    IsActive = true
                })
                .ToArray();

        public CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos)
            => DoRequest("v1/pairs")
                .Cast<dynamic>()
                .Select(x => new
                {
                    SourceSymbol = (string) x.currency1,
                    TargetSymbol = (string) x.currency2,
                    PairName = (string) x.name
                })
                .Where(x => x.TargetSymbol == "BTC")
                .Join(currencyInfos, x => x.SourceSymbol, x => x.Symbol, (x, y) => x)
                .Select(x => new
                {
                    x.SourceSymbol,
                    x.TargetSymbol,
                    Data = (dynamic) DoRequest($"charts/{x.PairName}/D/chart", "limit=1")[0]
                })
                .Select(x => new CurrencyMarketInfo
                {
                    IsActive = true,
                    SourceSymbol = x.SourceSymbol,
                    TargetSymbol = x.TargetSymbol,
                    LastPrice = (double) x.Data.close,
                    LowestAsk = (double) x.Data.close,
                    HighestBid = (double) x.Data.close,
                    LastDayLow = (double) x.Data.low,
                    LastDayHigh = (double) x.Data.high,
                    LastDayVolume = (double) x.Data.volume,
                })
                .ToArray();

        private JArray DoRequest(string command, string parameters = null)
        {
            var commandString = $"{command}/?format=json";
            if (parameters != null)
                commandString += $"&{parameters}";
            return JsonConvert.DeserializeObject<dynamic>(
                m_WebClient.DownloadString(new Uri(M_BaseUri, commandString)));
        }
    }
}
