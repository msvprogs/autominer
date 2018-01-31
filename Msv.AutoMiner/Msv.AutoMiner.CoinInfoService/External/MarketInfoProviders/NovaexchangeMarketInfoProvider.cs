using System;
using System.Linq;
using HtmlAgilityPack;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.HttpTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.MarketInfoProviders
{
    //API: https://novaexchange.com/remote/faq/
    public class NovaexchangeMarketInfoProvider : IMarketInfoProvider
    {
        private const double ConversionFeePercent = 0.2;
        private static readonly Uri[] M_BaseUris =
        {
            new Uri("https://novaexchange.com"),
            //use anonymiser to bypass IP range blocks
            new Uri("http://0s.nzxxmylfpbrwqylom5ss4y3pnu.cmle.ru")
        };

        public bool HasMarketsCountLimit => false;

        private readonly IWebClient m_WebClient;

        public NovaexchangeMarketInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public ExchangeCurrencyInfo[] GetCurrencies()
        {
            var currenciesHtml = new HtmlDocument();
            currenciesHtml.LoadHtml(DownloadString("/markets/"));
            return currenciesHtml.DocumentNode
                .SelectNodes("//h3[contains(.,'Trade these coins on Novaexchange')]/following-sibling::table/tr")
                .EmptyIfNull()
                .SelectMany(x => x.SelectNodes(".//td").EmptyIfNull())
                .Select(x => x.SelectNodes(".//span"))
                .Where(x => x?.Count >= 2)
                .Select(x => new ExchangeCurrencyInfo
                {
                    Symbol = x[0].InnerText,
                    Name = x[1].InnerText,
                    IsActive = true
                })
                .ToArray();
        }

        public CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos) 
            => DoRequest<JArray>("markets", "markets")
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

        private T DoRequest<T>(string command, string resultFieldKey)
            where T : JToken
        {
            var json = JsonConvert.DeserializeObject<JObject>(
                DownloadString($"/remote/v2/{command}"));
            if (json["status"].Value<string>() != "success")
                throw new ExternalDataUnavailableException(json["message"].Value<string>());
            return (T) json[resultFieldKey];
        }

        private string DownloadString(string relativeUrl)
        {
            Exception exception = null;
            var jsonStr = M_BaseUris
                .Select(x =>
                {
                    try
                    {
                        return m_WebClient.DownloadString(new Uri(x, relativeUrl));
                    }
                    catch (CorrectHttpException ex)
                    {
                        exception = ex;
                        return null;
                    }
                })
                .FirstOrDefault(x => x != null);
            if (jsonStr == null)
                throw exception ?? new ExternalDataUnavailableException("Unknown error");
            return jsonStr;
        }
    }
}
