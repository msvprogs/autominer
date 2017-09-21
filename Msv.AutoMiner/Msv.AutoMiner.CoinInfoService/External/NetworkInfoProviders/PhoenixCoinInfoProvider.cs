using System;
using System.Linq;
using HtmlAgilityPack;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    public class PhoenixCoinInfoProvider : TheBlockFactoryInfoProvider
    {
        private static readonly Uri M_BaseUri = new Uri("http://explorer.phoenixcoin.org");

        private readonly IWebClient m_WebClient;

        public PhoenixCoinInfoProvider(IWebClient webClient) 
            : base(webClient, "pxc")
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var stats = base.GetNetworkStats();
            var blocksPage = new HtmlDocument();
            blocksPage.LoadHtml(m_WebClient.DownloadString(new Uri(M_BaseUri, "/chain/Phoenixcoin")));
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
                    Timestamp = DateTimeHelper.TimestampFromIso8601DateTime(x.TimeElement)
                })
                .ToArray();
            var lastBlockHash = blocks.First(x => x.Height == stats.Height).Hash;
            var txPageHtml = new HtmlDocument();
            txPageHtml.LoadHtml(m_WebClient.DownloadString(new Uri(M_BaseUri, $"/block/{lastBlockHash}")));
            var generationElement = txPageHtml.DocumentNode.SelectSingleNode(
                "//td[contains(text(),'Generation') and contains(text(), 'total fees')]");
            stats.BlockReward = ParsingHelper.ParseGenerationReward(generationElement.InnerText);
            stats.BlockTimeSeconds = CalculateBlockStats(
                blocks.Select(x => new BlockInfo(x.Timestamp, x.Height)))?.MeanBlockTime;
            return stats;
        }
    }
}