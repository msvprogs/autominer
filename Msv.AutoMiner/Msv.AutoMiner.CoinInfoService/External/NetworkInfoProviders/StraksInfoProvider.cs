using System;
using HtmlAgilityPack;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    public class StraksInfoProvider : NetworkInfoProviderBase
    {
        private readonly IWebClient m_WebClient;

        public StraksInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var html = new HtmlDocument();
            html.LoadHtml(m_WebClient.DownloadString("https://straks.info"));
            return new CoinNetworkStatistics
            {
                BlockReward = 9.5, //constant
                NetHashRate = ParsingHelper.ParseHashRate(
                    html.DocumentNode.SelectSingleNode("//p[@id='hashrate']").InnerText),
                Difficulty = ParsingHelper.ParseDouble(
                    html.DocumentNode.SelectSingleNode("//p[@id='difficulty']").InnerText),
                Height = (long) ParsingHelper.ParseDouble(
                    html.DocumentNode.SelectSingleNode(
                        "//div[@id='blocks']//a[starts-with(@href, '/block/')]").InnerText)
            };
        }

    }
}
