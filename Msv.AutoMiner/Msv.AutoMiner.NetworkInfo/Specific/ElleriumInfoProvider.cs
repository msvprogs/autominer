using System;
using System.Globalization;
using HtmlAgilityPack;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Common;
using Msv.AutoMiner.NetworkInfo.Data;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    public class ElleriumInfoProvider : NetworkInfoProviderBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://elp.overemo.com/");

        private readonly IWebClient m_WebClient;

        public ElleriumInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var html = new HtmlDocument();
            html.LoadHtml(m_WebClient.DownloadString(M_BaseUri));

            return new CoinNetworkStatistics
            {
                Height = long.Parse(html.DocumentNode.SelectSingleNode(
                    "//div[contains(.,'Current Block')]/following-sibling::div").InnerText),
                Difficulty = ParsingHelper.ParseDouble(
                    html.DocumentNode.SelectSingleNode(
                        "//div[contains(.,'Difficulty')]/following-sibling::div").InnerText),
                NetHashRate = ParsingHelper.ParseHashRate(
                    html.DocumentNode.SelectSingleNode(
                        "//div[contains(.,'Network hash')]/following-sibling::div").InnerText),
                LastBlockTime = DateTime.ParseExact(html.DocumentNode.SelectSingleNode(
                            "//table[contains(@class, 'blocksTable')]//tr[@data-height][1]/td[2]/span")
                        .GetAttributeValue("title", null),
                    "yyyy-MM-dd HH:mm",
                    CultureInfo.InvariantCulture)
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(M_BaseUri, $"transaction/{hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(M_BaseUri, $"address/{address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(M_BaseUri, $"block/{blockHash}");
    }
}
