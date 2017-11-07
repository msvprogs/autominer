using System;
using System.Linq;
using HtmlAgilityPack;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    public class MinexCoinInfoProvider : INetworkInfoProvider
    {
        private static readonly Uri M_BaseUri = new Uri("https://minexexplorer.com");

        private readonly IWebClient m_WebClient;

        public MinexCoinInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public CoinNetworkStatistics GetNetworkStats()
        {
            var mainPage = new HtmlDocument();
            mainPage.LoadHtml(m_WebClient.DownloadString(M_BaseUri));
            var lastBlockLink = mainPage.DocumentNode.SelectSingleNode(
                "//table[@class='table main-table']//td[1]/a");

            var blockPage = new HtmlDocument();
            blockPage.LoadHtml(m_WebClient.DownloadString(
                new Uri(M_BaseUri, lastBlockLink.GetAttributeValue("href", null))));

            var height = long.Parse(lastBlockLink.InnerText);
            var txTable = blockPage.DocumentNode.SelectSingleNode(
                "//table[@class='table transactions-table'][1]");
            var reserve =
                ParsingHelper.ParseDouble(txTable.SelectSingleNode(".//div[@class='mnc-value'][2]").InnerText.Split()[0]);
            var totalCoinbase = ParsingHelper.ParseDouble(txTable.SelectSingleNode(".//div[@class='blue-bg']").InnerText.Split()[0]);

            var hashrate = JsonConvert.DeserializeObject<JArray>(
                    m_WebClient.DownloadString(new Uri(M_BaseUri, "hashrate.json")))
                .Last();
            var difficulty = JsonConvert.DeserializeObject<JArray>(
                    m_WebClient.DownloadString(new Uri(M_BaseUri, "difficulty.json")))
                .Last();

            return new CoinNetworkStatistics
            {
                Height = height,
                BlockReward = totalCoinbase - reserve,
                Difficulty = ((JArray) difficulty).Last.Value<double>(),
                NetHashRate = ((JArray) hashrate).Last.Value<double>()
            };
        }
    }
}
