using System;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using Msv.AutoMiner.Common;
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

            var lastPoWBlockLink = html.DocumentNode.SelectSingleNode(
                "//table[contains(@class, 'blocksTable')]//tr[@data-height and not(contains(.,'(PoS)'))][1]/td[1]/a");
            if (lastPoWBlockLink == null)
                throw new NoPoWBlocksException("No PoW blocks found among last 20 ones");
            var lastPoWBlockHtml = new HtmlDocument();
            lastPoWBlockHtml.LoadHtml(m_WebClient.DownloadString(
                new Uri(m_BaseUrl, lastPoWBlockLink.GetAttributeValue("href", null))));

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
                    CultureInfo.InvariantCulture),
                LastBlockTransactions = lastPoWBlockHtml.DocumentNode
                    .SelectNodes("//div[contains(@class, 'blockTx')]")
                    .EmptyIfNull()
                    .Select(x => new TransactionInfo
                    {
                        InValues = x.SelectNodes(".//div[@class='col-md-5']//td[@class='address' and not(contains(., 'Reward'))]/following-sibling::td")
                            .EmptyIfNull()
                            .Select(y => ParsingHelper.ParseDouble(y.InnerText))
                            .ToArray(),
                        OutValues = x.SelectNodes(".//div[@class='col-md-6']//td[@class='address']/following-sibling::td")
                            .EmptyIfNull()
                            .Select(y => ParsingHelper.ParseDouble(y.InnerText))
                            .ToArray()
                    })
                    .ToArray()
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
            => new Uri(m_BaseUrl, $"transaction/{hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(m_BaseUrl, $"address/{address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(m_BaseUrl, $"block/{blockHash}");
    }
}
