using System;
using HtmlAgilityPack;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Common;
using Msv.AutoMiner.NetworkInfo.Data;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    public class StraksInfoProvider : NetworkInfoProviderBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://straks.info/");

        private readonly IWebClient m_WebClient;

        public StraksInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var html = new HtmlDocument();
            html.LoadHtml(m_WebClient.DownloadString(M_BaseUri));
            var lastBlockLink = html.DocumentNode.SelectSingleNode(
                "//div[@id='blocks']//a[starts-with(@href, '/block/')]");
            return new CoinNetworkStatistics
            {
                BlockReward = 9.5, //constant
                NetHashRate = ParsingHelper.ParseHashRate(
                    html.DocumentNode.SelectSingleNode("//p[@id='hashrate']").InnerText),
                Difficulty = ParsingHelper.ParseDouble(
                    html.DocumentNode.SelectSingleNode("//p[@id='difficulty']").InnerText),
                Height = (long) ParsingHelper.ParseDouble(lastBlockLink.InnerText),
                LastBlockTime = DateTimeHelper.ToDateTimeUtc(
                    long.Parse(lastBlockLink.SelectSingleNode(".//ancestor::div/following-sibling::div")
                    .GetAttributeValue("data-time", null)))
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(M_BaseUri, $"transaction/{hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(M_BaseUri, $"address/{address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(M_BaseUri, $"block/{blockHash}");
    }
}
