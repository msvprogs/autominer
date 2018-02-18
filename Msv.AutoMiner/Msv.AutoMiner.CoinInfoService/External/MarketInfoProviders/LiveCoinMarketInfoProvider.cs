using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.MarketInfoProviders
{
    //API: https://www.livecoin.net/api/public
    public class LiveCoinMarketInfoProvider : IMarketInfoProvider
    {
        private static readonly Uri M_BaseUri = new Uri("https://api.livecoin.net");

        public bool HasMarketsCountLimit => false;
        public TimeSpan? RequestInterval => null;

        private readonly IWebClient m_WebClient;

        public LiveCoinMarketInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public ExchangeCurrencyInfo[] GetCurrencies() 
            => DoRequest<JArray>("info", "coinInfo")
                .Cast<dynamic>()
                .Select(x => new ExchangeCurrencyInfo
                {
                    Name = (string) x.name,
                    IsActive = (string) x.walletStatus == "normal",
                    Symbol = (string) x.symbol,
                    WithdrawalFee = (double) x.withdrawFee,
                    MinWithdrawAmount = (double) x.minWithdrawAmount
                })
                .ToArray();

        public CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos)
            => DoRequest<JArray>("exchange", "ticker")
                .Cast<dynamic>()
                .Select(x => new
                {
                    PairSymbols = ((string) x.symbol).Split('/'),
                    Data = x
                })
                .LeftOuterJoin(currencyInfos, x => x.PairSymbols[0], x => x.Symbol, 
                    (x, y) => (values: x, currency: y))
                .Select(x => new CurrencyMarketInfo
                {
                    IsActive = x.currency == null || x.currency.IsActive,
                    SourceSymbol = x.values.PairSymbols[0],
                    TargetSymbol = x.values.PairSymbols[1],
                    LastPrice = (double) x.values.Data.last,
                    HighestBid = (double) x.values.Data.min_ask,
                    LastDayHigh = (double) x.values.Data.high,
                    LastDayLow = (double) x.values.Data.low,
                    LastDayVolume = (double) x.values.Data.volume,
                    LowestAsk = (double) x.values.Data.max_bid
                })
                .ToArray();

        private T DoRequest<T>(string category, string command)
            where T : JToken
        {
            var json = JsonConvert.DeserializeObject<dynamic>(
                m_WebClient.DownloadString(new Uri(M_BaseUri, $"/{category}/{command}")));
            if (!(json is JObject) || json["success"] == null)
                return (T) json;
            if (!(bool)json.success)
                throw new ExternalDataUnavailableException();
            return (T)json[category];
        }
    }
}
