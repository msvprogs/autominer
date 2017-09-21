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
    //API: https://bittrex.com/Home/Api
    public class BittrexMarketInfoProvider : IMarketInfoProvider
    {
        private const double ConversionFeePercent = 0.25;
        private static readonly Uri M_BaseUri = new Uri("https://bittrex.com");

        public bool HasMarketsCountLimit => false;

        private readonly IWebClient m_WebClient;

        public BittrexMarketInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public ExchangeCurrencyInfo[] GetCurrencies()
            => DoRequest<JArray>("getcurrencies")
                .Cast<dynamic>()
                .Select(x => new ExchangeCurrencyInfo
                {
                    Name = (string) x.CurrencyLong,
                    IsActive = (bool) x.IsActive,
                    Symbol = (string) x.Currency,
                    WithdrawalFee = (double) x.TxFee
                })
                .ToArray();

        public CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos)
            => DoRequest<JArray>("getmarketsummaries")
                .Cast<dynamic>()
                .Select(x => new
                {
                    PairSymbols = ((string) x.MarketName).Split('-'),
                    Data = x
                })
                .Where(x => x.PairSymbols.Length > 1)
                .Select(x => new CurrencyMarketInfo
                {
                    SourceSymbol = x.PairSymbols[1],
                    TargetSymbol = x.PairSymbols[0],
                    HighestBid = (double) x.Data.Bid,
                    LowestAsk = (double) x.Data.Ask,
                    LastDayHigh = (double) x.Data.High,
                    LastDayLow = (double) x.Data.Low,
                    LastDayVolume = (double) x.Data.Volume,
                    LastPrice = (double)x.Data.Last,
                    SellFeePercent = ConversionFeePercent,
                    BuyFeePercent = ConversionFeePercent,
                    IsActive = true
                })
                .ToArray();

        private T DoRequest<T>(string command)
            where T : JToken
        {
            var json = JsonConvert.DeserializeObject<JObject>(
                m_WebClient.DownloadString(new Uri(M_BaseUri, $"/api/v1.1/public/{command}")));
            if (!json["success"].Value<bool>())
                throw new ExternalDataUnavailableException(json["message"].Value<string>());
            return (T) json["result"];
        }
    }
}
