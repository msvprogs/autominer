using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
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
                .Select(x => new CurrencyMarketInfo
                {
                    IsActive = true,
                    SourceSymbol = x.PairSymbols[0],
                    TargetSymbol = x.PairSymbols[1],
                    LastPrice = (double) x.Data.last,
                    HighestBid = (double) x.Data.max_bid,
                    LastDayHigh = (double) x.Data.high,
                    LastDayLow = (double) x.Data.low,
                    LastDayVolume = (double) x.Data.volume,
                    LowestAsk = (double) x.Data.min_ask,
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
