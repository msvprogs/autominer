using System;
using HtmlAgilityPack;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Service.External.Network
{
    //API: https://www.feathercoin.com/feathercoin-api/
    public class FeatherCoinInfoProvider : WebDownloaderBase, ICoinNetworkInfoProvider
    {
        private static readonly Uri M_ExplorerUri = new Uri("http://explorer.feathercoin.com");

        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic result = JsonConvert.DeserializeObject(
                DownloadString("http://api.feathercoin.com/?output=stats"));
            var searchHtml = new HtmlDocument();
            var height = (long) result.currblk;
            searchHtml.LoadHtml(DownloadString(
                new Uri(M_ExplorerUri, "/search?q=" + height).ToString()));
            var txRelativeUrl = searchHtml.DocumentNode.SelectSingleNode(
                "//a[contains(@href,'block')]").GetAttributeValue("href", string.Empty);
            var txUrl = new Uri(M_ExplorerUri, txRelativeUrl);

            var txHtml = new HtmlDocument();
            txHtml.LoadHtml(DownloadString(txUrl.ToString()));
            //<td>Generation: 80 + 0 total fees</td>
            var generationElement = txHtml.DocumentNode.SelectSingleNode(
                "//td[contains(text(),'Generation:') and contains(text(),'total fees')]");

            return new CoinNetworkStatistics
            {
                Difficulty = (double) result.nowdiff,
                NetHashRate = (long) ((double) result.khs * 1000),
                BlockTimeSeconds = (double)result.exptimeperblk,
                Height = height,
                BlockReward = ParsingHelper.ParseGenerationReward(generationElement.InnerText)
            };
        }
    }
}
