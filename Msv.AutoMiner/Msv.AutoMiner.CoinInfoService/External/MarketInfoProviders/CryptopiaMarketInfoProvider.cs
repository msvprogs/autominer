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
    //Public API: https://www.cryptopia.co.nz/Forum/Thread/255
    //Private API: https://www.cryptopia.co.nz/Forum/Thread/256
    public class CryptopiaMarketInfoProvider : IMarketInfoProvider
    {
        private static readonly Uri M_BaseUri = new Uri("https://www.cryptopia.co.nz");

        public bool HasMarketsCountLimit => false;
        public TimeSpan? RequestInterval => null;

        private readonly IWebClient m_WebClient;

        public CryptopiaMarketInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public ExchangeCurrencyInfo[] GetCurrencies()
            => DoRequest<JArray>("GetCurrencies")
                .Cast<dynamic>()
                .Select(x => new ExchangeCurrencyInfo
                {
                    IsActive = (string) x.Status == "OK",
                    MinWithdrawAmount = (double) x.MinWithdraw,
                    Name = (string) x.Name,
                    Symbol = (string) x.Symbol,
                    WithdrawalFee = (double) x.WithdrawFee
                })
                .ToArray();

        public CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos)
            => DoRequest<JArray>("GetMarkets")
                .Cast<dynamic>()
                .Join(DoRequest<JArray>("GetTradePairs").Cast<dynamic>(),
                    x => (string) x.Label,
                    x => (string) x.Label,
                    (x, y) => new
                    {
                        PairSymbols = ((string) x.Label).Split("/"),
                        MarketData = x,
                        TradePairData = y
                    })
                .Where(x => x.PairSymbols.Length > 1)
                .LeftOuterJoin(currencyInfos, x => x.PairSymbols[0], x => x.Symbol, 
                    (x, y) => (values: x, currency: y))
                .Select(x => new CurrencyMarketInfo
                {
                    SourceSymbol = x.values.PairSymbols[0],
                    TargetSymbol = x.values.PairSymbols[1],
                    HighestBid = (double) x.values.MarketData.BidPrice,
                    LowestAsk = (double) x.values.MarketData.AskPrice,
                    LastPrice = (double) x.values.MarketData.LastPrice,
                    LastDayLow = (double) x.values.MarketData.Low,
                    LastDayHigh = (double) x.values.MarketData.High,
                    LastDayVolume = (double) x.values.MarketData.Volume,
                    BuyFeePercent = (double) x.values.TradePairData.TradeFee,
                    SellFeePercent = (double)x.values.TradePairData.TradeFee,
                    IsActive = (x.currency == null || x.currency.IsActive)
                               && (string)x.values.TradePairData.Status == "OK"
                })
                .ToArray();

        private T DoRequest<T>(string command)
            where T : JToken
        {
            var json = JsonConvert.DeserializeObject<JObject>(
                m_WebClient.DownloadString(new Uri(M_BaseUri, $"/api/{command}")));
            if (!json["Success"].Value<bool>())
                throw new ExternalDataUnavailableException($"{json["Message"]} / {json["Error"]}");
            return (T) json["Data"];
        }
    }
}
