using System;
using HtmlAgilityPack;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;

namespace Msv.AutoMiner.NetworkInfo.Common
{
    public class AltmixInfoProvider : NetworkInfoProviderBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://altmix.org");

        private readonly IWebClient m_WebClient;
        private readonly string m_CurrencyName;

        public AltmixInfoProvider(IWebClient webClient, string currencyName)
        {
            if (string.IsNullOrEmpty(currencyName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(currencyName));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_CurrencyName = currencyName;
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var mainPage = new HtmlDocument();
            mainPage.LoadHtml(m_WebClient.DownloadString(new Uri(M_BaseUri, $"/coins/{m_CurrencyName}")));
            var infoNodes = mainPage.DocumentNode
                .SelectNodes("//tr[contains(.,'Blocks last 24h')]/following-sibling::tr/td");
            var lastBlockInfo = mainPage.DocumentNode
                .SelectSingleNode("//table[@class='table blocksTable']/tr[contains(.,'PoW')]");
            var lastBlockLink = lastBlockInfo.SelectSingleNode(".//td[1]/a");
            
            return new CoinNetworkStatistics
            {
                BlockTimeSeconds = 60 * ParsingHelper.ParseValueWithUnits(infoNodes[2].InnerText),
                NetHashRate = ParsingHelper.ParseHashRate(infoNodes[3].InnerText),
                Difficulty = ParsingHelper.ParseDouble(lastBlockInfo.SelectSingleNode(".//td[3]").InnerText),
                Height = long.Parse(lastBlockLink.InnerText),
                MoneySupply = ParsingHelper.ParseDouble(infoNodes[0].InnerText),
                LastBlockTime = DateTimeHelper.FromIso8601(
                    lastBlockInfo.SelectSingleNode(".//td[2]/span")?.GetAttributeValue("title", null))
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(CreateCurrencyBaseUrl(), "explorer/transaction/" + hash);

        public override Uri CreateAddressUrl(string address)
            => new Uri(CreateCurrencyBaseUrl(), "explorer/address/" + address);

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(CreateCurrencyBaseUrl(), "explorer/block/" + blockHash);

        private Uri CreateCurrencyBaseUrl()
            => new Uri(M_BaseUri, $"/coins/{m_CurrencyName}/");
    }
}
