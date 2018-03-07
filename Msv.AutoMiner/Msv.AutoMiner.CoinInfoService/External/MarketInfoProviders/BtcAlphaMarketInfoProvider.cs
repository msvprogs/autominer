using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Exchanges.Api;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.MarketInfoProviders
{
    public class BtcAlphaMarketInfoProvider : IMarketInfoProvider
    {
        public bool HasMarketsCountLimit => true;
        public TimeSpan? RequestInterval => null;

        private readonly IExchangeApi m_ExchangeApi;

        public BtcAlphaMarketInfoProvider(IExchangeApi exchangeApi)
            => m_ExchangeApi = exchangeApi ?? throw new ArgumentNullException(nameof(exchangeApi));

        public ExchangeCurrencyInfo[] GetCurrencies() 
            => DoRequest("v1/currencies")
                .Cast<dynamic>()
                .Select(x => new ExchangeCurrencyInfo
                {
                    Name = (string) x.short_name,
                    Symbol = (string) x.short_name,
                    IsActive = true
                })
                .ToArray();

        public CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos)
            => DoRequest("v1/pairs")
                .Cast<dynamic>()
                .Select(x => new
                {
                    SourceSymbol = (string) x.currency1,
                    TargetSymbol = (string) x.currency2,
                    PairName = (string) x.name
                })
                .Where(x => x.TargetSymbol == "BTC")
                .Join(currencyInfos, x => x.SourceSymbol, x => x.Symbol, (x, y) => x)
                .Select(x => new
                {
                    x.SourceSymbol,
                    x.TargetSymbol,
                    Data = (dynamic) DoRequest(
                        $"charts/{x.PairName}/D/chart", 
                        new Dictionary<string, string>
                        {
                            ["limit"] = "1"
                        })[0]
                })
                .Select(x => new CurrencyMarketInfo
                {
                    IsActive = true,
                    SourceSymbol = x.SourceSymbol,
                    TargetSymbol = x.TargetSymbol,
                    LastPrice = (double) x.Data.close,
                    LowestAsk = (double) x.Data.close,
                    HighestBid = (double) x.Data.close,
                    LastDayLow = (double) x.Data.low,
                    LastDayHigh = (double) x.Data.high,
                    LastDayVolume = (double) x.Data.volume,
                })
                .ToArray();

        private JArray DoRequest(string command, Dictionary<string, string> parameters = null) 
            => (JArray) m_ExchangeApi.ExecutePublic(command, parameters ?? new Dictionary<string, string>());
    }
}
