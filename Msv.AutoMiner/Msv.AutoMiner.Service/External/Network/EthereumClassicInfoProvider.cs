using HtmlAgilityPack;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;

namespace Msv.AutoMiner.Service.External.Network
{
    //Block explorer: https://gastracker.io/
    public class EthereumClassicInfoProvider : WebDownloaderBase, ICoinNetworkInfoProvider
    {
        public CoinNetworkStatistics GetNetworkStats()
        {
            var statsDocument = new HtmlDocument();
            statsDocument.LoadHtml(DownloadString("https://gastracker.io/"));
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
            lastBlockDocument.LoadHtml(DownloadString("https://gastracker.io/block/" + height));
            var reward = lastBlockDocument.DocumentNode.SelectSingleNode(
                ".//dt[text()='Miner Reward']/following-sibling::dd").InnerText.Trim();

            return new CoinNetworkStatistics
            {
                Difficulty = ParsingHelper.ParseHashRate(difficulty),
                NetHashRate = ParsingHelper.ParseHashRate(hashrate),
                Height = height,
                BlockTimeSeconds = ParsingHelper.ParseDouble(blockTime.Split()[0]),
                BlockReward = ParsingHelper.ParseDouble(reward.Split()[0])
            };
        }
    }
}
