using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly string m_ApiKey;
        private readonly string m_ApiSecret;

        public NovaexchangeMarketInfoProvider(IExchangeApi exchangeApi, string apiKey, string apiSecret)
        {
            m_ExchangeApi = exchangeApi ?? throw new ArgumentNullException(nameof(exchangeApi));
            m_ApiKey = apiKey;
            m_ApiSecret = apiSecret;
        }

        public ExchangeCurrencyInfo[] GetCurrencies()
        {
            if (string.IsNullOrEmpty(m_ApiKey) || string.IsNullOrEmpty(m_ApiSecret))
                return new ExchangeCurrencyInfo[0];

            // Method for getting wallet statuses is private for some reason...
            JArray walletStatuses = m_ExchangeApi.ExecutePrivate(
                "walletstatus", new Dictionary<string, string>(), m_ApiKey, Encoding.ASCII.GetBytes(m_ApiSecret)).coininfo;

            return walletStatuses
                .Cast<dynamic>()
                .Select(x => new ExchangeCurrencyInfo
                {
                    Symbol = (string) x.currency,
                    Name = (string) x.currencyname,
                    IsActive = (int) x.wallet_deposit == 1 && (int) x.wallet_status == 0,
                    ExternalId = (string) x.id,
                    WithdrawalFee = (double) x.wd_fee
                })
                .ToArray();
        }

        public CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos) 
            => ((JArray)m_ExchangeApi.ExecutePublic("markets", new Dictionary<string, string>()).markets)
                .Cast<dynamic>()
                .Join(currencyInfos, x => (string)x.currency, x => x.Symbol, (x, y) => (info:x, isActive:y.IsActive))
                .Select(x => new CurrencyMarketInfo
                {
                    SourceSymbol = (string)x.info.currency,
                    TargetSymbol = (string)x.info.basecurrency,
                    HighestBid = (double)x.info.bid,
                    LowestAsk = (double)x.info.ask,
                    LastPrice = (double)x.info.last_price,
                    LastDayLow = (double)x.info.low24h,
                    LastDayHigh = (double)x.info.high24h,
                    LastDayVolume = (double)x.info.volume24h,
                    IsActive = x.isActive, // don't know the meaning of this field: (int)x.disabled == 0,
                    BuyFeePercent = ConversionFeePercent,
                    SellFeePercent = ConversionFeePercent
                })
                .ToArray();
    }
}
