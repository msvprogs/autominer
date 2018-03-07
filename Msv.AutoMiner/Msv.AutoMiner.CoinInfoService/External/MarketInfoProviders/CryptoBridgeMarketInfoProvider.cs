using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Exchanges.Api;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.MarketInfoProviders
{
    public class CryptoBridgeMarketInfoProvider : IMarketInfoProvider
    {
        public bool HasMarketsCountLimit => false;
        public TimeSpan? RequestInterval => null;

        private readonly IExchangeApi m_ExchangeApi;

        public CryptoBridgeMarketInfoProvider(IExchangeApi exchangeApi)
            => m_ExchangeApi = exchangeApi ?? throw new ArgumentNullException(nameof(exchangeApi));

        public ExchangeCurrencyInfo[] GetCurrencies()
            => ((JArray)m_ExchangeApi.ExecutePublic("coins", new Dictionary<string, string>()))
                .Cast<dynamic>()
                .Select(x => new ExchangeCurrencyInfo
                {
                    Name = (string) x.name,
                    IsActive = (bool) x.depositAllowed && !(bool) x.restricted,
                    Symbol = ((string) x.backingCoinType)?.ToUpperInvariant() ?? (string) x.symbol,
                    WithdrawalFee = (double) x.transactionFee
                })
                .ToArray();

        public CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos)
            => ((JArray)m_ExchangeApi.ExecutePublic("ticker", new Dictionary<string, string>()))
                .Cast<dynamic>()
                .Select(x => new
                {
                    PairSymbols = ((string) x.id).Split('_'),
                    Data = x
                })
                .Where(x => x.PairSymbols.Length > 1)
                .Join(currencyInfos, x => x.PairSymbols[0], x => x.Symbol, 
                    (x, y) => (pair:x, currency:y))
                .Select(x => new CurrencyMarketInfo
                {
                    IsActive = x.currency.IsActive,
                    SourceSymbol = x.pair.PairSymbols[0],
                    TargetSymbol = x.pair.PairSymbols[1],
                    HighestBid = (double) x.pair.Data.bid,
                    //x.Data.volume is BTC volume
                    LastDayVolume = (double) x.pair.Data.volume / (double) x.pair.Data.last,
                    LowestAsk = (double) x.pair.Data.ask,
                    LastPrice = (double) x.pair.Data.last
                })
                .ToArray();
    }
}
