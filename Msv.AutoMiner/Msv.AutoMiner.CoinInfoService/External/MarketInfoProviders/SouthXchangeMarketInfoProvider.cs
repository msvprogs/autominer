using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Exchanges.Api;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.MarketInfoProviders
{
    public class SouthXchangeMarketInfoProvider : IMarketInfoProvider
    {
        public bool HasMarketsCountLimit => false;
        public TimeSpan? RequestInterval => null;

        private readonly IExchangeApi m_ExchangeApi;

        public SouthXchangeMarketInfoProvider(IExchangeApi exchangeApi)
            => m_ExchangeApi = exchangeApi ?? throw new ArgumentNullException(nameof(exchangeApi));

        public ExchangeCurrencyInfo[] GetCurrencies()
            => ((JArray)m_ExchangeApi.ExecutePublic("markets", new Dictionary<string, string>()))
                .Cast<JArray>()
                .Select(x => x[0].Value<string>())
                .Distinct()
                .Select(x => new ExchangeCurrencyInfo
                {
                    IsActive = true,
                    Symbol = x,
                    Name = x
                })
                .ToArray();

        public CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos)
            => ((JArray)m_ExchangeApi.ExecutePublic("prices", new Dictionary<string, string>()))
                .Cast<dynamic>()
                .Select(x => new
                {
                    PairSymbols = ((string) x.Market).Split('/'),
                    Data = x
                })
                .Where(x => x.PairSymbols.Length > 1)
                .Select(x => new CurrencyMarketInfo
                {
                    IsActive = true,
                    HighestBid = (double?) x.Data.Bid ?? 0,
                    LastDayVolume = (double?) x.Data.Volume24Hr ?? 0,
                    LastPrice = (double?) x.Data.Last ?? 0,
                    LowestAsk = (double?) x.Data.Ask ?? 0,
                    SourceSymbol = x.PairSymbols[0],
                    TargetSymbol = x.PairSymbols[1]
                })
                .ToArray();
    }
}
