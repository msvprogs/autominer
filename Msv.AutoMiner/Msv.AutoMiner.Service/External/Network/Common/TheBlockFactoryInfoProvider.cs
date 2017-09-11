using System;
using HtmlAgilityPack;
using Msv.AutoMiner.Service.Data;

namespace Msv.AutoMiner.Service.External.Network.Common
{
    public class TheBlockFactoryInfoProvider : NetworkInfoProviderBase
    {
        private readonly string m_SubPoolName;

        public TheBlockFactoryInfoProvider(string subPoolName)
        {
            if (string.IsNullOrEmpty(subPoolName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(subPoolName));
            m_SubPoolName = subPoolName;
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var result = DownloadString($"https://{m_SubPoolName}.theblocksfactory.com/statsAuth")
                .Replace("Difficulty</th>", "Difficulty</td>");
            var page = new HtmlDocument();
            page.LoadHtml(result);
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
