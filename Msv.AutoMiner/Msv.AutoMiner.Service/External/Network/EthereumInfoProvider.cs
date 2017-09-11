using System.Linq;
using System.Net;
using HtmlAgilityPack;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Service.External.Network
{
    //API: https://www.etherchain.org/documentation/api
    public class EthereumInfoProvider : WebDownloaderBase, ICoinNetworkInfoProvider
    {
        public CoinNetworkStatistics GetNetworkStats()
        {
            try
            {
                return GetFromEtherchain();
            }
            catch (WebException)
            {
                return GetFromEtherscan();
            }
        }

        private CoinNetworkStatistics GetFromEtherchain()
        {
            dynamic statsJson = JsonConvert.DeserializeObject(
                DownloadString("https://etherchain.org/api/miningEstimator"));
            var data = statsJson.data[0];
            dynamic heightJson = JsonConvert.DeserializeObject(
                DownloadString("https://etherchain.org/api/blocks/count"));
            var height = (long) heightJson.data[0].count;
            dynamic blockJson = JsonConvert.DeserializeObject(
                DownloadString("https://etherchain.org/api/block/" + height));
            return new CoinNetworkStatistics
            {
                Difficulty = (double) data.difficulty,
                NetHashRate = (long) data.hashRate,
                BlockTimeSeconds = (double) data.blockTime,
                Height = height,
                BlockReward = ((long) blockJson.data[0].reward - (long) blockJson.data[0].totalFee) / 1e18
            };
        }

        private CoinNetworkStatistics GetFromEtherscan()
        {
            var mainPage = new HtmlDocument();
            mainPage.LoadHtml(DownloadString("https://etherscan.io/"));
            var height = mainPage.DocumentNode.SelectSingleNode("//a[@title='The latest block no']").InnerText.Trim();
            var blockTime = mainPage.DocumentNode.SelectSingleNode("//span[@title='Avg Time of the last 5000 Blocks']")
                .InnerText.TrimStart('(').Split('s')[0];
            var hashRate = mainPage.DocumentNode
                .SelectSingleNode("//span[@title='Avg Hash Rate of the last 5000 Blocks']").InnerText.Trim();
            var difficulty = mainPage.DocumentNode
                .SelectSingleNode("//span[@title='Average Difficulty']").InnerText.Trim();
            var blockReward = mainPage.DocumentNode
                .SelectSingleNode("//p[contains(text(),'Block Reward')]").InnerText.Trim()
                .Split().Reverse().Skip(1).First();
            return new CoinNetworkStatistics
            {
                BlockReward = ParsingHelper.ParseDouble(blockReward),
                BlockTimeSeconds = ParsingHelper.ParseDouble(blockTime),
                Difficulty = ParsingHelper.ParseHashRate(difficulty),
                Height = long.Parse(height),
                NetHashRate = ParsingHelper.ParseHashRate(hashRate)
            };
        }
    }
}
