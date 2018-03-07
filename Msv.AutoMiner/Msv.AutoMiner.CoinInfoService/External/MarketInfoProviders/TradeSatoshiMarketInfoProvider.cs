using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Exchanges.Api;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.MarketInfoProviders
{
    public class TradeSatoshiMarketInfoProvider : IMarketInfoProvider
    {
        private const double ConversionFeePercent = 0.2;

        public bool HasMarketsCountLimit => false;
        public TimeSpan? RequestInterval => null;

        private readonly IExchangeApi m_ExchangeApi;

        public TradeSatoshiMarketInfoProvider(IExchangeApi exchangeApi)
            => m_ExchangeApi = exchangeApi ?? throw new ArgumentNullException(nameof(exchangeApi));

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
                .LeftOuterJoin(currencyInfos, x => x.PairSymbols[0], x => x.Symbol, 
                    (x, y) => (values: x, currency: y))
                .Select(x => new CurrencyMarketInfo
                {
                    SourceSymbol = x.values.PairSymbols[0],
                    TargetSymbol = x.values.PairSymbols[1],
                    HighestBid = (double) x.values.Data.bid,
                    LowestAsk = (double) x.values.Data.ask,
                    LastDayHigh = (double) x.values.Data.high,
                    LastDayLow = (double) x.values.Data.low,
                    LastPrice = (double) x.values.Data.last,
                    LastDayVolume = (double) x.values.Data.volume,
                    IsActive = x.currency == null || x.currency.IsActive,
                    SellFeePercent = ConversionFeePercent,
                    BuyFeePercent = ConversionFeePercent
                })
                .ToArray();

        private T DoRequest<T>(string command)
            where T : JToken
            => (T) m_ExchangeApi.ExecutePublic(command, new Dictionary<string, string>());
    }
}
