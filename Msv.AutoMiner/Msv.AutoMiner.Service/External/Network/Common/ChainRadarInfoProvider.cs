using System;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.Infrastructure;

namespace Msv.AutoMiner.Service.External.Network.Common
{
    public class ChainRadarInfoProvider : NetworkInfoProviderBase
    {
        private readonly string m_CurrencySymbol;

        public ChainRadarInfoProvider(string currencySymbol)
        {
            if (string.IsNullOrEmpty(currencySymbol))
                throw new ArgumentException("Value cannot be null or empty.", nameof(currencySymbol));

            m_CurrencySymbol = currencySymbol.ToLowerInvariant();
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var blocksPage = new HtmlDocument();
            blocksPage.LoadHtml(DownloadString($"http://chainradar.com/{m_CurrencySymbol}/blocks"));

            var chainInfoTable = blocksPage.DocumentNode.SelectSingleNode("//table[@id='chain-info']");
            var hashrateSection =
                chainInfoTable.SelectSingleNode(".//td[contains(text(),'hashrate')]/following-sibling::td");
            var difficultySection =
                chainInfoTable.SelectSingleNode(".//td[contains(text(),'Difficulty')]/following-sibling::td");
            var blocksSection =
                blocksPage.DocumentNode.SelectNodes("//tbody[@id='blocks-tbody']/tr");
            var blocks = blocksSection
                .Select(x => new BlockInfo
                {
                    Height = long.Parse(x.SelectSingleNode(".//td[1]/a").InnerText),
                    Timestamp = TimestampHelper.ToTimestamp(DateTime.ParseExact(
                            x.SelectSingleNode(".//td[2]").InnerText,
                            "yyyy-MM-dd HH:mm:ss",
                            CultureInfo.InvariantCulture),
                        TimeZoneInfo.Utc)
                })
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
