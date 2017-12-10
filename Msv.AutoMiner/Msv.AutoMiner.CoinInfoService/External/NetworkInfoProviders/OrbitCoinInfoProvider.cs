using System;
using HtmlAgilityPack;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    public class OrbitCoinInfoProvider : NetworkInfoProviderBase
    {
        private readonly IWebClient m_WebClient;

        public OrbitCoinInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var stats = new HtmlDocument();
            stats.LoadHtml(m_WebClient.DownloadString("http://www.dnb.io/wp-content/uploads/orbmonitor.php"));
            var infoBlockItems = stats.DocumentNode.SelectNodes("//ul[@id='orbmonitor']/li[@class='row number']");
            return new CoinNetworkStatistics
            {
                Difficulty = ParsingHelper.ParseDouble(infoBlockItems[2].InnerText),
                Height = ParsingHelper.ParseLong(infoBlockItems[0].InnerText),
                NetHashRate = ParsingHelper.ParseHashRate(infoBlockItems[1].InnerText)
            };
        }
    }
}
