using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Exchanges.Api;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.MarketInfoProviders
{
    public class NovaexchangeMarketInfoProvider : IMarketInfoProvider
    {
        private const double ConversionFeePercent = 0.2;

        public bool HasMarketsCountLimit => false;
        public TimeSpan? RequestInterval => null;

        private readonly IExchangeApi m_ExchangeApi;

        public NovaexchangeMarketInfoProvider(IExchangeApi exchangeApi)
            => m_ExchangeApi = exchangeApi ?? throw new ArgumentNullException(nameof(exchangeApi));

        public ExchangeCurrencyInfo[] GetCurrencies()
        {
            //TODO: add currencies
            return new ExchangeCurrencyInfo[0];
        }

        public CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos) 
            => ((JArray)m_ExchangeApi.ExecutePublic("markets", new Dictionary<string, string>()).markets)
                .Cast<dynamic>()
                .Select(x => new CurrencyMarketInfo
                {
                    SourceSymbol = (string)x.currency,
                    TargetSymbol = (string)x.basecurrency,
                    HighestBid = (double)x.bid,
                    LowestAsk = (double)x.ask,
                    LastPrice = (double)x.last_price,
                    LastDayLow = (double)x.low24h,
                    LastDayHigh = (double)x.high24h,
                    LastDayVolume = (double)x.volume24h,
                    IsActive = (int)x.disabled == 0,
                    BuyFeePercent = ConversionFeePercent,
                    SellFeePercent = ConversionFeePercent
                })
                .ToArray();
    }
}
