using System;
using HtmlAgilityPack;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    //Block explorer: https://gastracker.io/
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

            return new CoinNetworkStatistics
            {
                Difficulty = ParsingHelper.ParseHashRate(difficulty),
                NetHashRate = ParsingHelper.ParseHashRate(hashrate),
                Height = height,
                BlockTimeSeconds = ParsingHelper.ParseValueWithUnits(blockTime),
                BlockReward = ParsingHelper.ParseValueWithUnits(reward)
            };
        }
    }
}
