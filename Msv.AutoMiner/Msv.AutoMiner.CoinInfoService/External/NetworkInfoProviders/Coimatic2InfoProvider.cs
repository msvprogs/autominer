using System;
using HtmlAgilityPack;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    public class Coimatic2InfoProvider : IquidusWithPosDifficultyInfoProvider
    {
        private readonly IWebClient m_WebClient;

        public Coimatic2InfoProvider(IWebClient webClient)
            : base(webClient, "http://195.181.247.196:3001/")
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public override CoinNetworkStatistics GetNetworkStats()
        {
            CoinNetworkStatistics stats;
            try
            {
                stats = base.GetNetworkStats();
            }
            catch
            {
                stats = GetAlternativeNetworkStats();
            }
            stats.BlockReward = GetBlockRewardFromHeight(stats.Height);
            return stats;
        }

        private CoinNetworkStatistics GetAlternativeNetworkStats()
        {
            var html = new HtmlDocument();
            html.LoadHtml(m_WebClient.DownloadString("http://198.199.90.93/Coimatic2/index.php"));
            return new CoinNetworkStatistics
            {
                Height = long.Parse(html.DocumentNode
                    .SelectSingleNode("//td[contains(.,'Blocks:')]/following-sibling::td/a").InnerText),
                Difficulty = ParsingHelper.ParseDouble(
                    html.DocumentNode.SelectSingleNode("//td[contains(.,'Difficulty')]/following-sibling::td").InnerText
                        .Split()[0]),
                NetHashRate = ParsingHelper.ParseHashRate(
                    html.DocumentNode.SelectSingleNode("//td[contains(.,'Network Hashrate:')]/following-sibling::td")
                        .InnerText)
            };
        }

        private static double GetBlockRewardFromHeight(long height)
        {
            if (height == 1)
                return 17000000;
            if (height <= 170002)
                return 250;
            if (height <= 340002)
                return 125;
            if (height <= 510002)
                return 62.5;
            if (height <= 850002)
                return 31.25;
            return 0;
        }
    }
}
