using System;
using HtmlAgilityPack;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Newtonsoft.Json;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    //API: https://www.feathercoin.com/feathercoin-api/
    public class FeatherCoinInfoProvider : INetworkInfoProvider
    {
        private static readonly Uri M_BaseUri = new Uri("http://explorer.feathercoin.com");

        private readonly IWebClient m_WebClient;

        public FeatherCoinInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic stats = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString("http://api.feathercoin.com/?output=stats"));
            var searchHtml = new HtmlDocument();
            var height = (long) stats.currblk;
            searchHtml.LoadHtml(m_WebClient.DownloadString(
                new Uri(M_BaseUri, "/search?q=" + height).ToString()));
            var txRelativeUrl = searchHtml.DocumentNode.SelectSingleNode(
                "//a[contains(@href,'block')]").GetAttributeValue("href", string.Empty);
            var txUrl = new Uri(M_BaseUri, txRelativeUrl);

            var txHtml = new HtmlDocument();
            txHtml.LoadHtml(m_WebClient.DownloadString(txUrl.ToString()));
            //<td>Generation: 80 + 0 total fees</td>
            var generationElement = txHtml.DocumentNode.SelectSingleNode(
                "//td[contains(text(),'Generation:') and contains(text(),'total fees')]");

            return new CoinNetworkStatistics
            {
                Difficulty = (double) stats.nowdiff,
                NetHashRate = (long) ((double) stats.khs * 1000),
                BlockTimeSeconds = (double)stats.exptimeperblk,
                Height = height,
                BlockReward = ParsingHelper.ParseGenerationReward(generationElement.InnerText)
            };
        }
    }
}
