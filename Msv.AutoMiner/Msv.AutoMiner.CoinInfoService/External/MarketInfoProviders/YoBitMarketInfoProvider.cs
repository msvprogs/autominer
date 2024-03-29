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
    public class YoBitMarketInfoProvider : IMarketInfoProvider
    {
        private const double ConversionFeePercent = 0.2;
        private const string BtcMarketPostfix = "_btc";

        public bool HasMarketsCountLimit => true;
        public TimeSpan? RequestInterval => null;

        private readonly IExchangeApi m_ExchangeApi;

        public YoBitMarketInfoProvider(IExchangeApi exchangeApi)
            => m_ExchangeApi = exchangeApi ?? throw new ArgumentNullException(nameof(exchangeApi));

        public ExchangeCurrencyInfo[] GetCurrencies()
        {
            JObject pairs = m_ExchangeApi.ExecutePublic("info", new Dictionary<string, string>()).pairs;
            return pairs.Properties()
                .Where(x => x.Name.EndsWith(BtcMarketPostfix, StringComparison.CurrentCultureIgnoreCase))
                .Cast<dynamic>()
                .Select(x => new ExchangeCurrencyInfo
                {
                    Symbol = ((string) x.Name).Split("_")[0].ToUpperInvariant(),
                    IsActive = (int) x.Value.hidden == 0,
                    Name = string.Empty
                })
                .ToArray();
        }

        public CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos)
        {
            if (!currencyInfos.Any())
                return new CurrencyMarketInfo[0];

            var pairs = string.Join("-", currencyInfos.Select(x => x.Symbol + BtcMarketPostfix)).ToLowerInvariant();
            return ((JObject) m_ExchangeApi.ExecutePublic($"ticker/{pairs}", new Dictionary<string, string>
                {
                    ["ignore_invalid"] = "1"
                }))
                .Properties()
                .Select(x => new
                {
                    PairSymbols = x.Name.ToUpperInvariant().Split("_"),
                    Data = (dynamic) x.Value
                })
                .Where(x => x.PairSymbols.Length > 1)
                .LeftOuterJoin(currencyInfos, x => x.PairSymbols[0], x => x.Symbol,
                    (x, y) => (values: x, currency: y))
                .Select(x => new CurrencyMarketInfo
                {
                    SourceSymbol = x.values.PairSymbols[0],
                    TargetSymbol = x.values.PairSymbols[1],
                    HighestBid = (double) x.values.Data.buy,
                    LowestAsk = (double) x.values.Data.sell,
                    LastDayHigh = (double) x.values.Data.high,
                    LastDayLow = (double) x.values.Data.low,
                    //'vol' field is in BTC
                    LastDayVolume = (double) x.values.Data.vol_cur,
                    LastPrice = (double) x.values.Data.last,
                    IsActive = x.currency == null || x.currency.IsActive,
                    SellFeePercent = ConversionFeePercent,
                    BuyFeePercent = ConversionFeePercent
                })
                .ToArray();
        }
    }
}
