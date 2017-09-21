using System;
using System.Linq;
using HtmlAgilityPack;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Newtonsoft.Json;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    public class SibCoinInfoProvider : NetworkInfoProviderBase
    {
        private readonly IWebClient m_WebClient;

        public SibCoinInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public override CoinNetworkStatistics GetNetworkStats()
        {
            dynamic json = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString("http://sib.miningclub.info/global_stats"));
            var transactionPage = new HtmlDocument();
            transactionPage.LoadHtml(m_WebClient.DownloadString("https://chain.sibcoin.net/en"));
            var blocks = transactionPage.DocumentNode.SelectNodes("//tr")
                .Where(x => x.SelectSingleNode(".//td") != null)
                .Select(x => new BlockInfo(
                    long.Parse(x.SelectSingleNode(".//td[1]/a").InnerText),
                    DateTimeHelper.TimestampFromRussianDateTime(x.SelectSingleNode(".//td[3]").InnerText)))
                .ToArray();
            var firstBlockPage = new HtmlDocument();
            firstBlockPage.LoadHtml(m_WebClient.DownloadString("https://chain.sibcoin.net/en/b/" + blocks[0].Height));
            //<li>Generation 22.056129263797 + -2.83444208 total fees</li>
            var rewardElement = firstBlockPage.DocumentNode
                .SelectSingleNode("//li[contains(text(),'Generation') and contains(text(), 'total fees')]");
            return new CoinNetworkStatistics
            {
                Difficulty = (double) json.network_block_difficulty,
                NetHashRate = (long) json.network_hashrate,
                BlockTimeSeconds = CalculateBlockStats(blocks)?.MeanBlockTime,
                BlockReward = ParsingHelper.ParseGenerationReward(rewardElement.InnerText),
                Height = blocks[0].Height
            };
        }
    }
}
