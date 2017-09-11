using System;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Network.Common;
using Msv.AutoMiner.Service.Infrastructure;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Service.External.Network
{
    public class SibCoinInfoProvider : NetworkInfoProviderBase
    {
        public override CoinNetworkStatistics GetNetworkStats()
        {
            dynamic json = JsonConvert.DeserializeObject(
                DownloadString("http://sib.miningclub.info/global_stats"));
            var transactionPage = new HtmlDocument();
            transactionPage.LoadHtml(DownloadString("https://chain.sibcoin.net/en"));
            var blocks = transactionPage.DocumentNode.SelectNodes("//tr")
                .Where(x => x.SelectSingleNode(".//td") != null)
                .Select(x => new BlockInfo
                {
                    Height = long.Parse(x.SelectSingleNode(".//td[1]/a").InnerText),
                    Timestamp = TimestampHelper.ToTimestamp(DateTime.ParseExact(
                            x.SelectSingleNode(".//td[3]").InnerText,
                            "dd.MM.yyyy HH:mm:ss",
                            CultureInfo.InvariantCulture),
                        TimeZoneInfo.Utc)
                })
                .ToArray();
            var firstBlockPage = new HtmlDocument();
            firstBlockPage.LoadHtml(DownloadString("https://chain.sibcoin.net/en/b/" + blocks[0].Height));
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
