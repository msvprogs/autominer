using HtmlAgilityPack;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Network.Common;

namespace Msv.AutoMiner.Service.External.Network
{
    public class CryptoniteInfoProvider : NetworkInfoProviderBase
    {
        public override CoinNetworkStatistics GetNetworkStats()
        {
            var html = new HtmlDocument();
            html.LoadHtml(DownloadString("http://xcn-explorer.selektion21.de/?page=stats"));

            return new CoinNetworkStatistics
            {
                BlockReward = ParsingHelper.ParseDouble(GetNumericValue(html, "Block Reward")),
                Difficulty = ParsingHelper.ParseDouble(GetNumericValue(html, "Difficulty")),
                Height = long.Parse(GetNumericValue(html, "Block Count")),
                BlockTimeSeconds = 60 * ParsingHelper.ParseDouble(GetNumericValue(html, "Avg. Block Time")),
                NetHashRate = ParsingHelper.ParseHashRate(GetNumericValue(html, "Hash Rate", true))
            };
        }

        private static string GetNumericValue(HtmlDocument document, string label, bool ignoreSpace = false)
        {
            var text = document.DocumentNode
                .SelectSingleNode($"//td[contains(.,'{label}')]/following-sibling::td")?.InnerText?.Trim();
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            return text.Contains(" ") && !ignoreSpace
                ? text.Split()[0] 
                : text;
        }
    }
}
