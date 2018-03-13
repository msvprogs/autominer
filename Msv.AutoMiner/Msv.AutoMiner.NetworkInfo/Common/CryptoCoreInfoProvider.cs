using System;
using System.Globalization;
using HtmlAgilityPack;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;

namespace Msv.AutoMiner.NetworkInfo.Common
{
    public class CryptoCoreInfoProvider : NetworkInfoProviderBase
    {
        private readonly IWebClient m_WebClient;
        private readonly Uri m_BaseUrl;

        public CryptoCoreInfoProvider(IWebClient webClient, string baseUrl)
        {
            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_BaseUrl = new Uri(baseUrl);
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var html = new HtmlDocument();
            html.LoadHtml(m_WebClient.DownloadString(m_BaseUrl));

            var lastPoWBlockUrl = html.DocumentNode.SelectSingleNode(
                "//table[contains(@class, 'blocksTable')]//tr[@data-height and not(contains(.,'(PoS)'))][1]/td[1]/a")
                .GetAttributeValue("href", null);
            var lastPoWBlockHtml = new HtmlDocument();
            lastPoWBlockHtml.LoadHtml(m_WebClient.DownloadString(new Uri(m_BaseUrl, lastPoWBlockUrl)));

            return new CoinNetworkStatistics
            {
                Height = long.Parse(html.DocumentNode.SelectSingleNode(
                    "//div[contains(.,'Current Block')]/following-sibling::div").InnerText),
                Difficulty = ParsingHelper.ParseDouble(
                    lastPoWBlockHtml.DocumentNode.SelectSingleNode(
                        "//table[contains(.,'Difficulty')]//tr[2]/td[2]").InnerText),
                NetHashRate = ParsingHelper.ParseHashRate(
                    html.DocumentNode.SelectSingleNode(
                        "//div[contains(.,'Network hash')]/following-sibling::div").InnerText),
                LastBlockTime = DateTime.ParseExact(
                    html.DocumentNode.SelectSingleNode(
                        "//table[contains(@class, 'blocksTable')]//tr[@data-height][1]/td[2]/span")
                        .GetAttributeValue("title", null),
                    "yyyy-MM-dd HH:mm",
                    CultureInfo.InvariantCulture)
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(m_BaseUrl, $"transaction/{hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(m_BaseUrl, $"address/{address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(m_BaseUrl, $"block/{blockHash}");
    }
}
