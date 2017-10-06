﻿using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.MarketInfoProviders
{
    //API: https://tradesatoshi.com/Home/Api
    public class TradeSatoshiMarketInfoProvider : IMarketInfoProvider
    {
        private const double ConversionFeePercent = 0.2;
        private static readonly Uri M_BaseUri = new Uri("https://tradesatoshi.com");

        public bool HasMarketsCountLimit => false;

        private readonly IWebClient m_WebClient;

        public TradeSatoshiMarketInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public ExchangeCurrencyInfo[] GetCurrencies()
            => DoRequest<JArray>("getcurrencies")
                .Cast<dynamic>()
                .Select(x => new ExchangeCurrencyInfo
                {
                    Symbol = (string) x.currency,
                    Name = (string) x.currencyLong,
                    IsActive = (string) x.status == "OK",
                    WithdrawalFee = (double) x.txFee
                })
                .ToArray();

        public CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos)
            => DoRequest<JArray>("getmarketsummaries")
                .Cast<dynamic>()
                .Select(x => new
                {
                    PairSymbols = ((string) x.market).Split("_"),
                    Data = x
                })
                .Where(x => x.PairSymbols.Length > 1)
                .Select(x => new CurrencyMarketInfo
                {
                    SourceSymbol = x.PairSymbols[0],
                    TargetSymbol = x.PairSymbols[1],
                    HighestBid = (double) x.Data.bid,
                    LowestAsk = (double) x.Data.ask,
                    LastDayHigh = (double) x.Data.high,
                    LastDayLow = (double) x.Data.low,
                    LastPrice = (double) x.Data.last,
                    LastDayVolume = (double) x.Data.volume,
                    IsActive = true,
                    SellFeePercent = ConversionFeePercent,
                    BuyFeePercent = ConversionFeePercent
                })
                .ToArray();

        private T DoRequest<T>(string command)
            where T : JToken
        {
            var json = JsonConvert.DeserializeObject<JObject>(
                m_WebClient.DownloadString(new Uri(M_BaseUri, $"/api/public/{command}")));
            if (!json["success"].Value<bool>())
                throw new ExternalDataUnavailableException(json["message"].Value<string>());
            return (T)json["result"];
        }
    }
}