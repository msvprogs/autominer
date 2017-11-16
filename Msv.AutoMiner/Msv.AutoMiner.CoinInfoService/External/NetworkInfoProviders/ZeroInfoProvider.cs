using System;
using HtmlAgilityPack;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    public class ZeroInfoProvider : NetworkInfoProviderBase
    {
        private static readonly Uri M_BaseUrl = new Uri("https://zeroexplorer.com");

        private readonly IWebClient m_WebClient;

        public ZeroInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var mainPage = new HtmlDocument();
            mainPage.LoadHtml(m_WebClient.DownloadString(M_BaseUrl));
            var dataTable = mainPage.DocumentNode.SelectSingleNode("//div[@class='table-responsive']/table");
            
            var blockPage = new HtmlDocument();
            blockPage.LoadHtml(m_WebClient.DownloadString(new Uri(M_BaseUrl, $"/?block={GetFieldValue(dataTable, "Last block hash")}")));

            return new CoinNetworkStatistics
            {
                BlockReward = ParsingHelper.ParseValueWithUnits(
                    blockPage.DocumentNode
                        .SelectSingleNode("//tr[contains(.,'Newly Generated Coins (Block Reward)')]/td[3]/span")
                        .InnerText),
                Difficulty = ParsingHelper.ParseDouble(GetFieldValue(dataTable, "Difficulty")),
                Height = long.Parse(GetFieldValue(dataTable, "Height")),
                NetHashRate = ParsingHelper.ParseHashRate(GetFieldValue(dataTable, "Network hashrate"))
            };
        }

        private static string GetFieldValue(HtmlNode dataTable, string fieldName) 
            => dataTable.SelectSingleNode($".//th[contains(., '{fieldName}')]/following-sibling::td").InnerText?.Trim();
    }
}
