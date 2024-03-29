﻿using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Exchanges.Api;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.MarketInfoProviders
{
    public class BittrexMarketInfoProvider : IMarketInfoProvider
    {
        private const double ConversionFeePercent = 0.25;

        public bool HasMarketsCountLimit => false;
        public TimeSpan? RequestInterval => null;

        private readonly IExchangeApi m_ExchangeApi;

        public BittrexMarketInfoProvider(IExchangeApi exchangeApi)
            => m_ExchangeApi = exchangeApi ?? throw new ArgumentNullException(nameof(exchangeApi));

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
                .LeftOuterJoin(currencyInfos, x => x.PairSymbols[1], x => x.Symbol, 
                    (x, y) => (values: x, currency: y))
                .Select(x => new CurrencyMarketInfo
                {
                    SourceSymbol = x.values.PairSymbols[1],
                    TargetSymbol = x.values.PairSymbols[0],
                    HighestBid = (double) x.values.Data.Bid,
                    LowestAsk = (double) x.values.Data.Ask,
                    LastDayHigh = (double) x.values.Data.High,
                    LastDayLow = (double) x.values.Data.Low,
                    LastDayVolume = (double) x.values.Data.Volume,
                    LastPrice = (double)x.values.Data.Last,
                    SellFeePercent = ConversionFeePercent,
                    BuyFeePercent = ConversionFeePercent,
                    IsActive = x.currency == null || x.currency.IsActive
                })
                .ToArray();

        private T DoRequest<T>(string command)
            where T : JToken 
            => (T) m_ExchangeApi.ExecutePublic(command, new Dictionary<string, string>());
    }
}
