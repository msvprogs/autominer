using System;
using HtmlAgilityPack;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common
{
    public class TheBlockFactoryInfoProvider : NetworkInfoProviderBase
    {
        private readonly IWebClient m_WebClient;
        private readonly string m_SubPoolName;

        public TheBlockFactoryInfoProvider(IWebClient webClient, string subPoolName)
        {
            if (string.IsNullOrEmpty(subPoolName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(subPoolName));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_SubPoolName = subPoolName;
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var pageHtml = m_WebClient.DownloadString($"https://{m_SubPoolName}.theblocksfactory.com/statsAuth")
                .Replace("Difficulty</th>", "Difficulty</td>");
            var page = new HtmlDocument();
            page.LoadHtml(pageHtml);
            var difficulty = ParsingHelper.ParseDouble(page.DocumentNode.SelectSingleNode(
                "//td[contains(text(),'Current Difficulty')]/following-sibling::td").InnerText);
            var hashrate = ParsingHelper.ParseHashRate(page.DocumentNode.SelectSingleNode(
                "//td[contains(text(), 'Network Hashrate')]/following-sibling::td").InnerText);
            var height = page.DocumentNode.SelectSingleNode(
                "//font[contains(text(), '(Current:')]/a").InnerText;
            return new CoinNetworkStatistics
            {
                Difficulty = difficulty,
                NetHashRate = hashrate,
                Height = long.Parse(height.TrimEnd(')'))
            };
        }
    }
}
