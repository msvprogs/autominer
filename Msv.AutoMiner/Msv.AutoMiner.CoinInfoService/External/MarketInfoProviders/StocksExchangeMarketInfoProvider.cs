using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.MarketInfoProviders
{
    //API: https://docs.google.com/document/d/1mU8ecTlzfDtT1hmZJ-dXezMudLnfD4ZeNBr_oxFwdGI/edit#
    public class StocksExchangeMarketInfoProvider : IMarketInfoProvider
    {
        private static readonly Uri M_BaseUri = new Uri("https://stocks.exchange/api2/");

        public bool HasMarketsCountLimit => false;

        public TimeSpan? RequestInterval => TimeSpan.FromMinutes(1);

        private readonly IWebClient m_WebClient;

        public StocksExchangeMarketInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public ExchangeCurrencyInfo[] GetCurrencies() 
            => JsonConvert.DeserializeObject<JArray>(
                m_WebClient.DownloadString(new Uri(M_BaseUri, "currencies")))
            .Cast<dynamic>()
            .Select(x => new ExchangeCurrencyInfo
            {
                Symbol = (string) x.currency,
                Name = (string) x.currency_long,
                IsActive = (bool) x.active,
                MinWithdrawAmount = (double) x.minimum_withdrawal_amount,
                WithdrawalFee = (double) x.withdrawal_fee_const
            })
            .ToArray();

        public CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos)
        {
            var marketData = JsonConvert.DeserializeObject<JArray>(
                    m_WebClient.DownloadString(new Uri(M_BaseUri, "markets")))
                .Cast<dynamic>()
                .Select(x => new
                {
                    Source = (string) x.currency,
                    Target = (string) x.partner,
                    Data = x
                })
                .GroupBy(x => new {x.Source, x.Target})
                .ToDictionary(x => x.Key, x => x.First().Data);

            return JsonConvert.DeserializeObject<JArray>(
                    m_WebClient.DownloadString(new Uri(M_BaseUri, "ticker")))
                .Cast<dynamic>()
                .Select(x => new
                {
                    PairSymbols = ((string) x.market_name).Split("_"),
                    Data = x
                })
                .Where(x => x.PairSymbols.Length > 1)
                .LeftOuterJoin(
                    marketData, 
                    x => new {Source = x.PairSymbols[0], Target = x.PairSymbols[1]},
                    x => x.Key,
                    (x, y) => (ticker: x, market: y.Value))
                .Join(currencyInfos, x => x.ticker.PairSymbols[0], x => x.Symbol, 
                    (x, y) => (x.ticker, x.market, currency: y))
                .Select(x => new CurrencyMarketInfo
                {
                    IsActive = x.currency.IsActive && (x.market == null || (bool)x.market.active),
                    SourceSymbol = x.ticker.PairSymbols[0],
                    TargetSymbol = x.ticker.PairSymbols[1],
                    LastPrice = (double)x.ticker.Data.last,
                    HighestBid = (double)x.ticker.Data.bid,
                    LastDayVolume = (double)x.ticker.Data.vol,
                    LowestAsk = (double)x.ticker.Data.ask,
                    LastDayLow = 0, //(double)x.ticker.Data.low,
                    LastDayHigh = 0, //(double)x.ticker.Data.high,
                    BuyFeePercent = (double)x.ticker.Data.buy_fee_percent,
                    SellFeePercent = (double)x.ticker.Data.sell_fee_percent,
                })
                .ToArray();
        }
    }
}
