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
    public class LiveCoinMarketInfoProvider : IMarketInfoProvider
    {
        public bool HasMarketsCountLimit => false;
        public TimeSpan? RequestInterval => null;

        private readonly IExchangeApi m_ExchangeApi;

        public LiveCoinMarketInfoProvider(IExchangeApi exchangeApi)
            => m_ExchangeApi = exchangeApi ?? throw new ArgumentNullException(nameof(exchangeApi));

        public ExchangeCurrencyInfo[] GetCurrencies() 
            => ((JArray)m_ExchangeApi.ExecutePublic("info/coinInfo", new Dictionary<string, string>()).info)
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
            => ((JArray)m_ExchangeApi.ExecutePublic("exchange/ticker", new Dictionary<string, string>()))
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
    }
}
