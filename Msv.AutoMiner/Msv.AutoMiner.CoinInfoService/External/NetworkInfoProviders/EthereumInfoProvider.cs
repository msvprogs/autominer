using System;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Newtonsoft.Json;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    //API: https://www.etherchain.org/documentation/api
    public class EthereumInfoProvider : INetworkInfoProvider
    {
        private static readonly Uri M_EtherchainBaseUri = new Uri("https://etherchain.org");

        private readonly IWebClient m_WebClient;

        public EthereumInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

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
                m_WebClient.DownloadString(new Uri(M_EtherchainBaseUri, "/api/miningEstimator")));
            var data = statsJson.data[0];
            dynamic heightJson = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_EtherchainBaseUri, "/api/blocks/count")));
            var height = (long) heightJson.data[0].count;
            dynamic blockJson = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_EtherchainBaseUri, "/api/block/" + height)));
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
            mainPage.LoadHtml(m_WebClient.DownloadString("https://etherscan.io/"));
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
