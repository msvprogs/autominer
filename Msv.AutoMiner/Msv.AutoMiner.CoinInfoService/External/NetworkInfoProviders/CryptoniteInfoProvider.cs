using System;
using HtmlAgilityPack;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    public class CryptoniteInfoProvider : NetworkInfoProviderBase
    {
        private readonly IWebClient m_WebClient;

        public CryptoniteInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var html = new HtmlDocument();
            html.LoadHtml(m_WebClient.DownloadString("http://xcn-explorer.selektion21.de/?page=stats"));

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
