using System;
using System.Linq;
using HtmlAgilityPack;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    //Block explorer: https://gastracker.io/
    [SpecificCoinInfoProvider("ETC")]
    public class EthereumClassicInfoProvider : INetworkInfoProvider
    {
        private static readonly Uri M_BaseUri = new Uri("https://gastracker.io/");

        private readonly IWebClient m_WebClient;

        public EthereumClassicInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public CoinNetworkStatistics GetNetworkStats()
        {
            var statsDocument = new HtmlDocument();
            statsDocument.LoadHtml(m_WebClient.DownloadString(M_BaseUri));
            var statsNode = statsDocument.DocumentNode
                .SelectSingleNode("//div/h3[text()='Blockchain']/following-sibling::dl");
            var difficulty = statsNode.SelectSingleNode(
                ".//dt[text()='Difficulty']/following-sibling::dd").InnerText;
            var hashrate = statsNode.SelectSingleNode(
                ".//dt[text()='Hashrate']/following-sibling::dd").InnerText;
            var height = long.Parse(statsNode.SelectSingleNode(
                ".//dt[text()='Height']/following-sibling::dd").InnerText);
            var blockTime = statsDocument.DocumentNode.SelectSingleNode(
                ".//dt[text()='Avg Block Time']/following-sibling::dd").InnerText.Trim();

            var lastBlockDocument = new HtmlDocument();
            lastBlockDocument.LoadHtml(m_WebClient.DownloadString(new Uri(M_BaseUri, $"/block/{height}")));
            var reward = lastBlockDocument.DocumentNode.SelectSingleNode(
                ".//dt[text()='Miner Reward']/following-sibling::dd").InnerText.Trim();
            var lastBlockTime = DateTimeHelper.FromIso8601(string.Join(" ", lastBlockDocument.DocumentNode
                .SelectSingleNode(".//dd[@class='timestamp']/span")
                .GetAttributeValue("data-original-title", "")
                .Trim()
                .Split()
                .Take(2)));

            return new CoinNetworkStatistics
            {
                Difficulty = ParsingHelper.ParseHashRate(difficulty),
                NetHashRate = ParsingHelper.ParseHashRate(hashrate),
                Height = height,
                BlockTimeSeconds = ParsingHelper.ParseValueWithUnits(blockTime),
                BlockReward = ParsingHelper.ParseValueWithUnits(reward),
                LastBlockTime = lastBlockTime
            };
        }

        public WalletBalance GetWalletBalance(string address)
        {
            throw new NotImplementedException();
        }

        public BlockExplorerWalletOperation[] GetWalletOperations(string address, DateTime startDate)
        {
            throw new NotImplementedException();
        }

        public Uri CreateTransactionUrl(string hash)
            => new Uri(M_BaseUri, $"tx/{hash}");

        public Uri CreateAddressUrl(string address)
            => new Uri(M_BaseUri, $"addr/{address}");

        public Uri CreateBlockUrl(string blockHash)
            => new Uri(M_BaseUri, $"block/{blockHash}");
    }
}
