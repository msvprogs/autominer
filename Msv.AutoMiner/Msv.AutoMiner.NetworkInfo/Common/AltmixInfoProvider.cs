using System;
using System.Linq;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;

namespace Msv.AutoMiner.NetworkInfo.Common
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
            var mainPage = m_WebClient.DownloadHtml(new Uri(M_BaseUri, $"/coins/{m_CurrencyName}"));

            var alerts = mainPage.DocumentNode.SelectNodes("//div[contains(@class, 'alert ')]");
            if (alerts != null && alerts.Select(x => x.InnerText?.Trim())
                    .Any(x => x == "Coin not working" || x == "Sorry, the coin under maintenance. Try again later."))
                throw new ExternalDataUnavailableException("Explorer for this coin is inactive");

            var infoNodes = mainPage.DocumentNode
                .SelectNodes("//tr[contains(.,'Blocks last 24h')]/following-sibling::tr/td");
            var lastBlockInfo = mainPage.DocumentNode
                .SelectSingleNode("//table[@class='table blocksTable']/tr[contains(.,'PoW')]");
            var lastBlockLink = lastBlockInfo.SelectSingleNode(".//td[1]/a");

            var lastBlock = m_WebClient.DownloadHtml(lastBlockLink.GetAttributeValue("href", null));

            var infoHas5Cols = infoNodes.Count == 5;
            return new CoinNetworkStatistics
            {
                BlockTimeSeconds = 60 * ParsingHelper.ParseValueWithUnits(infoHas5Cols 
                                       ? infoNodes[2].InnerText
                                       : infoNodes[1].InnerText),
                NetHashRate = infoHas5Cols 
                    ? ParsingHelper.ParseHashRate(infoNodes[3].InnerText) 
                    : 0,
                Difficulty = ParsingHelper.ParseDouble(lastBlockInfo.SelectSingleNode(".//td[3]").InnerText),
                Height = long.Parse(lastBlockLink.InnerText),
                TotalSupply = infoHas5Cols 
                    ? ParsingHelper.ParseDouble(infoNodes[0].InnerText) 
                    : (double?)null,
                LastBlockTime = DateTimeHelper.FromIso8601(
                    lastBlockInfo.SelectSingleNode(".//td[2]/span")?.GetAttributeValue("title", null)),
                LastBlockTransactions = lastBlock.DocumentNode
                    .SelectNodes("//div[contains(@class, 'blockTx')]")
                    .EmptyIfNull()
                    .Select(x => new TransactionInfo
                    {
                        InValues = x.SelectNodes(".//div[@class='col-md-5']//td[@class='address' and not(contains(., 'Generation'))]/following-sibling::td")
                            .EmptyIfNull()
                            .Select(y => ParsingHelper.ParseDouble(y.InnerText))
                            .ToArray(),
                        OutValues = x.SelectNodes(".//div[@class='col-md-6']//td[@class='address']/following-sibling::td")
                            .EmptyIfNull()
                            .Select(y => ParsingHelper.ParseDouble(y.InnerText))
                            .ToArray()
                    })
                    .ToArray()
            };
        }

        public override WalletBalance GetWalletBalance(string address)
        {
            throw new NotImplementedException();
        }

        public override BlockExplorerWalletOperation[] GetWalletOperations(string address, DateTime startDate)
        {
            throw new NotImplementedException();
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(CreateCurrencyBaseUrl(), "explorer/transaction/" + hash);

        public override Uri CreateAddressUrl(string address)
            => new Uri(CreateCurrencyBaseUrl(), "explorer/address/" + address);

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(CreateCurrencyBaseUrl(), "explorer/block/" + blockHash);

        private Uri CreateCurrencyBaseUrl()
            => new Uri(M_BaseUri, $"/coins/{m_CurrencyName}/");
    }
}
