using System;
using System.Linq;
using HtmlAgilityPack;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common
{
    public class AltmixInfoProvider : NetworkInfoProviderBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://altmix.org");

        private readonly IWebClient m_WebClient;
        private readonly string m_CurrencyName;

        public AltmixInfoProvider(IWebClient webClient, string currencyName)
        {
            if (string.IsNullOrEmpty(currencyName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(currencyName));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_CurrencyName = currencyName;
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var mainPage = new HtmlDocument();
            mainPage.LoadHtml(m_WebClient.DownloadString(new Uri(M_BaseUri, "/coins/" + m_CurrencyName)));
            var infoNodes = mainPage.DocumentNode
                .SelectNodes("//tr[contains(.,'Blocks last 24h')]/following-sibling::tr/td");
            var lastBlockInfo = mainPage.DocumentNode
                .SelectSingleNode("//table[@class='table blocksTable']/tr[contains(.,'PoW')]");
            var lastBlockLink = lastBlockInfo.SelectSingleNode(".//td[1]/a");

            var blockPage = new HtmlDocument();
            blockPage.LoadHtml(m_WebClient.DownloadString(lastBlockLink.GetAttributeValue("href", null)));
            var blockRewardNode = blockPage.DocumentNode
                .SelectNodes("//div[@class='blockTx well'][1]//div[@class='col-md-6']//td[@class='number']")
                .Last();

            return new CoinNetworkStatistics
            {
                BlockReward = ParsingHelper.ParseDouble(blockRewardNode.InnerText),
                BlockTimeSeconds = 60 * ParsingHelper.ParseValueWithUnits(infoNodes[1].InnerText),
                Difficulty = ParsingHelper.ParseDouble(lastBlockInfo.SelectSingleNode(".//td[3]").InnerText),
                Height = long.Parse(lastBlockLink.InnerText)
            };
        }
    }
}
