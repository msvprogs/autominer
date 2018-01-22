using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.MarketInfoProviders
{
    //API: https://poloniex.com/support/api/
    public class PoloniexMarketInfoProvider : IMarketInfoProvider
    {
        private const double ConversionFeePercent = 0.25;
        private static readonly Uri M_BaseUri = new Uri("https://poloniex.com");

        public bool HasMarketsCountLimit => false;

        private readonly IWebClient m_WebClient;

        public PoloniexMarketInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public ExchangeCurrencyInfo[] GetCurrencies() 
            => DoRequest<JObject>("returnCurrencies")
                .Cast<dynamic>()
                .Select(x => new ExchangeCurrencyInfo
                {
                    Name = (string) x.Value.Name,
                    Symbol = x.Name,
                    WithdrawalFee = (double) x.Value.txFee,
                    IsActive = (int) x.Value.disabled == 0 && (int) x.Value.delisted == 0 && (int) x.Value.frozen == 0
                })
                .ToArray();

        public CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos) 
            => DoRequest<JObject>("returnTicker")
                .Properties()
                .Select(x => new
                {
                    PairSymbols = x.Name.Split("_"),
                    Data = (dynamic) x.Value
                })
                .Where(x => x.PairSymbols.Length > 1)
                .LeftOuterJoin(currencyInfos, x => x.PairSymbols[1], x => x.Symbol, 
                    (x, y) => (values: x, currency: y))
                .Select(x => new CurrencyMarketInfo
                {
                    SourceSymbol = x.values.PairSymbols[1],
                    TargetSymbol = x.values.PairSymbols[0],
                    IsActive = (x.currency == null || x.currency.IsActive) 
                               && (int) x.values.Data.isFrozen == 0,
                    HighestBid = (double) x.values.Data.highestBid,
                    LowestAsk = (double) x.values.Data.lowestAsk,
                    LastPrice = (double) x.values.Data.last,
                    LastDayHigh = (double) x.values.Data.high24hr,
                    LastDayLow = (double) x.values.Data.low24hr,
                    LastDayVolume = (double) x.values.Data.quoteVolume,
                    BuyFeePercent = ConversionFeePercent,
                    SellFeePercent = ConversionFeePercent
                })
                .ToArray();

        private T DoRequest<T>(string command)
            where T : JToken => 
            JsonConvert.DeserializeObject<T>(m_WebClient.DownloadString(new Uri(M_BaseUri, $"/public?command={command}")));
    }
}
