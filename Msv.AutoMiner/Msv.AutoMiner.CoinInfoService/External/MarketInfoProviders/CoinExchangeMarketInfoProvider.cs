using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.External;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.MarketInfoProviders
{
    //API: http://coinexchangeio.github.io/slate
    public class CoinExchangeMarketInfoProvider : IMarketInfoProvider
    {
        //private static readonly Uri M_BaseUri = new Uri("https://www.coinexchange.io");
        private static readonly Uri M_BaseUri = new Uri("https://www.coinexchange2.com/");

        public bool HasMarketsCountLimit => false;

        private readonly IWebClient m_WebClient;

        public CoinExchangeMarketInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public ExchangeCurrencyInfo[] GetCurrencies()
            => new ExchangeCurrencyInfo[0];

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
                            IsActive = (bool) x.Active
                        }),
                    x => (int) x.MarketID,
                    x => x.Id,
                    (x, y) => new {Data = x, MarketInfo = y})
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
                        IsActive = x.MarketInfo.IsActive
                    })
                .ToArray();

        private JArray DoRequest(string command)
        {
            dynamic response = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_BaseUri, $"/api/v1/{command}")));
            if ((int) response.success != 1)
                throw new ExternalDataUnavailableException((string) response.message);
            return response.result;
        }
    }
}
