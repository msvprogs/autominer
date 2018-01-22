using System;
using HtmlAgilityPack;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Common;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    public class CryptoniteInfoProvider : NetworkInfoProviderBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://explorer.digicent.org/");

        private readonly IWebClient m_WebClient;

        public CryptoniteInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var html = new HtmlDocument();
            html.LoadHtml(m_WebClient.DownloadString(new Uri(M_BaseUri, "?page=stats")));

            var bestBlockHash = m_WebClient.DownloadString(new Uri(M_BaseUri, "?q=getlasthash"));
            dynamic bestBlockInfo = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_BaseUri, "?q=blockinfo&arg1=" + bestBlockHash)));

            return new CoinNetworkStatistics
            {
                BlockReward = ParsingHelper.ParseDouble(GetNumericValue(html, "Block Reward")),
                Difficulty = ParsingHelper.ParseDouble(GetNumericValue(html, "Difficulty")),
                Height = long.Parse(GetNumericValue(html, "Block Count")),
                BlockTimeSeconds = 60 * ParsingHelper.ParseDouble(GetNumericValue(html, "Avg. Block Time")),
                NetHashRate = ParsingHelper.ParseHashRate(GetNumericValue(html, "Hash Rate", true)),
                LastBlockTime = DateTimeHelper.ToDateTimeUtc((long)bestBlockInfo.time)
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(M_BaseUri, $"?tx={hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(M_BaseUri, $"?address={address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(M_BaseUri, $"?block={blockHash}");

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
