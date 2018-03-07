using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Exchanges.Api;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.MarketInfoProviders
{
    public class CoinExchangeMarketInfoProvider : IMarketInfoProvider
    {
        public bool HasMarketsCountLimit => false;
        public TimeSpan? RequestInterval => null;

        private readonly IExchangeApi m_ExchangeApi;

        public CoinExchangeMarketInfoProvider(IExchangeApi exchangeApi)
            => m_ExchangeApi = exchangeApi ?? throw new ArgumentNullException(nameof(exchangeApi));

        public ExchangeCurrencyInfo[] GetCurrencies()
            => DoRequest("getcurrencies")
                .Cast<dynamic>()
                .Select(x => new ExchangeCurrencyInfo
                {
                    Name = (string) x.Name,
                    ExternalId = (string) x.CurrencyID,
                    IsActive = (string) x.WalletStatus == "online",
                    Symbol = (string) x.TickerCode
                })
                .ToArray();

        public CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos)
            => DoRequest("getmarketsummaries")
                .Cast<dynamic>()
                .Join(DoRequest("getmarkets")
                        .Cast<dynamic>()
                        .Where(x => (bool) x.Active)
                        .Select(x => new
                        {
                            Id = (int) x.MarketID,
                            SourceSymbol = (string) x.MarketAssetCode,
                            TargetSymbol = (string) x.BaseCurrencyCode,
                            IsActive = (bool) x.Active,
                            SourceId = (string) x.MarketAssetID
                        }),
                    x => (int) x.MarketID,
                    x => x.Id,
                    (x, y) => new {Data = x, MarketInfo = y})
                .Join(currencyInfos, x => x.MarketInfo.SourceId, x => x.ExternalId,
                    (x, y) => new {x.MarketInfo, x.Data, CurrencyInfo = y})
                .Select(x => new CurrencyMarketInfo
                {
                    SourceSymbol = x.MarketInfo.SourceSymbol,
                    TargetSymbol = x.MarketInfo.TargetSymbol,
                    BuyFeePercent = 0,
                    SellFeePercent = 0,
                    LowestAsk = (double) x.Data.AskPrice,
                    HighestBid = (double) x.Data.BidPrice,
                    LastDayLow = (double) x.Data.LowPrice,
                    LastDayHigh = (double) x.Data.HighPrice,
                    // 'Volume' field returns the same BTC volume
                    LastDayVolume = (double) x.Data.BTCVolume / (double) x.Data.LastPrice,
                    LastPrice = (double) x.Data.LastPrice,
                    IsActive = x.CurrencyInfo.IsActive && x.MarketInfo.IsActive
                })
                .ToArray();

        private JArray DoRequest(string command) 
            => (JArray) m_ExchangeApi.ExecutePublic(command, new Dictionary<string, string>());
    }
}
