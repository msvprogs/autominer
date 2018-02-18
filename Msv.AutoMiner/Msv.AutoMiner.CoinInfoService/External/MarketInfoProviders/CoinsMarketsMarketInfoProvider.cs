using System;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.MarketInfoProviders
{
    public class CoinsMarketsMarketInfoProvider : IMarketInfoProvider
    {
        private const double ConversionFeePercent = 0.25;

        private static readonly Uri M_BaseUri = new Uri("https://coinsmarkets.com");
        private static readonly Regex M_NameRegex = new Regex(
            @"(?<name>\w+)\s*\(\s*(?<symbol>)\w+\s*\)", RegexOptions.Compiled);

        public bool HasMarketsCountLimit => false;
        public TimeSpan? RequestInterval => null;

        private readonly IWebClient m_WebClient;

        public CoinsMarketsMarketInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public ExchangeCurrencyInfo[] GetCurrencies()
        {
            var feesHtml = new HtmlDocument();
            feesHtml.LoadHtml(m_WebClient.DownloadString(new Uri(M_BaseUri, "/fees.php")));
            return feesHtml.DocumentNode
                .SelectNodes("//h1[contains(.,'What Are the withdraw')]/following-sibling::table//tr")
                .EmptyIfNull()
                .Select(x => x.SelectNodes(".//td"))
                .Where(x => x?.Count >= 3)
                .Select(x => new
                {
                    NameMatch = M_NameRegex.Match(x[0].InnerText),
                    Fee = ParsingHelper.ParseDouble(x[1].InnerText),
                    MinAmount = ParsingHelper.ParseDouble(x[2].InnerText)
                })
                .Where(x => x.NameMatch.Success)
                .Select(x => new ExchangeCurrencyInfo
                {
                    IsActive = true,
                    Name = x.NameMatch.Groups["name"].Value,
                    Symbol = x.NameMatch.Groups["symbol"].Value.ToUpperInvariant(),
                    WithdrawalFee = x.Fee,
                    MinWithdrawAmount = x.MinAmount
                })
                .ToArray();
        }

        public CurrencyMarketInfo[] GetCurrencyMarkets(ExchangeCurrencyInfo[] currencyInfos)
            => JsonConvert.DeserializeObject<JObject>(m_WebClient.DownloadString(new Uri(M_BaseUri, "/apicoin.php")))
                .Properties()
                .Select(x => new
                {
                    PairSymbols = x.Name.ToUpperInvariant().Split('_'),
                    Data = (dynamic) x.Value
                })
                .Where(x => x.PairSymbols.Length > 1)
                .Select(x => new CurrencyMarketInfo
                {
                    SourceSymbol = x.PairSymbols[1],
                    TargetSymbol = x.PairSymbols[0],
                    HighestBid = (double) x.Data.highestBid,
                    LowestAsk = (double) x.Data.lowestAsk,
                    LastPrice = (double) x.Data.last,
                    LastDayLow = (double) x.Data.low24hr,
                    LastDayHigh = (double) x.Data.high24hr,
                    LastDayVolume = ((JToken)x.Data["24htrade"]).Value<double>() / (double)x.Data.last,
                    BuyFeePercent = ConversionFeePercent,
                    SellFeePercent = ConversionFeePercent,
                    IsActive = true
                })
                .ToArray();
    }
}
