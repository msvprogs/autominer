using System;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Network.Common;
using Msv.AutoMiner.Service.Infrastructure;

namespace Msv.AutoMiner.Service.External.Network
{
    public class PhoenixCoinInfoProvider : TheBlockFactoryInfoProvider
    {
        public PhoenixCoinInfoProvider() 
            : base("pxc")
        { }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var stats = base.GetNetworkStats();
            var blocksPage = new HtmlDocument();
            blocksPage.LoadHtml(DownloadString("http://explorer.phoenixcoin.org/chain/Phoenixcoin"));
            var blocks = blocksPage.DocumentNode.SelectNodes("//tr")
                .Where(x => x.SelectSingleNode(".//td") != null)
                .Select(x => new
                {
                    BlockLinkElement = x.SelectSingleNode(".//td[1]/a"),
                    TimeElement = x.SelectSingleNode(".//td[2]").InnerText
                })
                .Select(x => new
                {
                    Height = long.Parse(x.BlockLinkElement.InnerText),
                    Hash = new Uri(new Uri("http://example.com"),
                        x.BlockLinkElement.GetAttributeValue("href", null)).Segments.Last(),
                    Timestamp = TimestampHelper.ToTimestamp(DateTime.ParseExact(
                            x.TimeElement,
                            "yyyy-MM-dd HH:mm:ss",
                            CultureInfo.InvariantCulture),
                        TimeZoneInfo.Utc)
                })
                .ToArray();
            var lastBlockHash = blocks.First(x => x.Height == stats.Height).Hash;
            var txPageHtml = new HtmlDocument();
            txPageHtml.LoadHtml(DownloadString("http://explorer.phoenixcoin.org/block/" + lastBlockHash));
            var generationElement = txPageHtml.DocumentNode.SelectSingleNode(
                "//td[contains(text(),'Generation') and contains(text(), 'total fees')]");
            stats.BlockReward = ParsingHelper.ParseGenerationReward(generationElement.InnerText);
            stats.BlockTimeSeconds = CalculateBlockStats(
                blocks.Select(x => new BlockInfo
                {
                    Height = x.Height,
                    Timestamp = x.Timestamp
                }))?.MeanBlockTime;
            return stats;
        }
    }
}
