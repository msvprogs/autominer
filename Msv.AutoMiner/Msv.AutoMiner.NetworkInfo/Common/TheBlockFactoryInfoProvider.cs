using System;
using HtmlAgilityPack;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;

namespace Msv.AutoMiner.NetworkInfo.Common
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

        public override WalletBalance GetWalletBalance(string address)
        {
            throw new NotImplementedException();
        }

        public override BlockExplorerWalletOperation[] GetWalletOperations(string address, DateTime startDate)
        {
            throw new NotImplementedException();
        }

        public override Uri CreateTransactionUrl(string hash)
            => null;

        public override Uri CreateAddressUrl(string address)
            => null;

        public override Uri CreateBlockUrl(string blockHash)
            => null;
    }
}
