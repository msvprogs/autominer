using System;
using System.Linq;
using HtmlAgilityPack;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common
{
    public class ChainRadarInfoProvider : NetworkInfoProviderBase
    {
        private readonly IWebClient m_WebClient;
        private readonly string m_CurrencySymbol;

        public ChainRadarInfoProvider(IWebClient webClient, string currencySymbol)
        {
            if (string.IsNullOrEmpty(currencySymbol))
                throw new ArgumentException("Value cannot be null or empty.", nameof(currencySymbol));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_CurrencySymbol = currencySymbol.ToLowerInvariant();
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var blocksPage = new HtmlDocument();
            blocksPage.LoadHtml(m_WebClient.DownloadString($"http://chainradar.com/{m_CurrencySymbol}/blocks"));

            var chainInfoTable = blocksPage.DocumentNode.SelectSingleNode("//table[@id='chain-info']");
            var hashrateSection =
                chainInfoTable.SelectSingleNode(".//td[contains(text(),'hashrate')]/following-sibling::td");
            var difficultySection =
                chainInfoTable.SelectSingleNode(".//td[contains(text(),'Difficulty')]/following-sibling::td");
            var blocksSection =
                blocksPage.DocumentNode.SelectNodes("//tbody[@id='blocks-tbody']/tr");
            var blocks = blocksSection
                .Select(x => new BlockInfo(
                    DateTimeHelper.TimestampFromIso8601DateTime(
                        x.SelectSingleNode(".//td[2]").InnerText),
                    long.Parse(x.SelectSingleNode(".//td[1]/a").InnerText)))
                .ToArray();
            return new CoinNetworkStatistics
            {
                BlockTimeSeconds = CalculateBlockStats(blocks)?.MeanBlockTime,
                Height = blocks.Max(x => x.Height),
                Difficulty = ParsingHelper.ParseDouble(difficultySection.InnerText),
                NetHashRate = ParsingHelper.ParseHashRate(hashrateSection.InnerText)
            };
        }
    }
}
