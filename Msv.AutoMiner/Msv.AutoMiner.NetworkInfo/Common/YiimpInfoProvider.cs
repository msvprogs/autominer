using System;
using System.Linq;
using System.Threading;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.NetworkInfo.Common
{
    public class YiimpInfoProvider : NetworkInfoProviderBase
    {
        private readonly IWebClient m_WebClient;
        private readonly Uri m_ExplorerUri;

        public YiimpInfoProvider(IWebClient webClient, string baseUrl, string currencySymbol)
        {
            if (baseUrl == null) 
                throw new ArgumentNullException(nameof(baseUrl));
            if (currencySymbol == null)
                throw new ArgumentNullException(nameof(currencySymbol));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_ExplorerUri = new Uri(new Uri(baseUrl), $"/explorer/{currencySymbol}");
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var mainPage = m_WebClient.DownloadHtml(m_ExplorerUri);
            // Standard columns:
            // Age	Height	Difficulty	    Type	Tx	Conf	Blockhash
            // 6m	1193	362.07963742311	PoW	    1	2	    00000000007b170bb6761a1c4614979faeb1162f52c4b202077278d695b85301

            var lastPoWBlock = mainPage.DocumentNode.SelectNodes("//tr[@class='ssrow']")
                .EmptyIfNull()
                .FirstOrDefault(x => x.SelectSingleNode(".//td[4]")?.InnerText.Trim() == "PoW");
            if (lastPoWBlock == null)
                throw new NoPoWBlocksException("Not found PoW blocks among last 20");

            // Yiimp pools have request per second limit
            Thread.Sleep(600);
            var lastBlockHtml = m_WebClient.DownloadHtml(
                CreateBlockUrl(lastPoWBlock.SelectSingleNode(".//td[7]").InnerText.Trim()));

            var transactionJsons = lastBlockHtml.DocumentNode
                .SelectNodes("//table[@class='dataGrid']//tr[@class='raw']")
                .EmptyIfNull()
                .Select(x => JsonConvert.DeserializeObject<dynamic>(x.InnerText))
                .ToArray();

            return new CoinNetworkStatistics
            {
                LastBlockTime = DateTimeHelper.FromIso8601(
                    lastPoWBlock.SelectSingleNode(".//td[1]/span").GetAttributeValue("title", "")),
                Height = ParsingHelper.ParseLong(lastPoWBlock.SelectSingleNode(".//td[2]").InnerText),
                Difficulty = ParsingHelper.ParseDouble(lastPoWBlock.SelectSingleNode(".//td[3]").InnerText),
                LastBlockTransactions = transactionJsons
                    .Select(x => new TransactionInfo
                    {
                        IsCoinbase = ((JArray) x.vin)
                            .Cast<dynamic>()
                            .Any(y => y.coinbase != null),
                        InValues = ((JArray) x.vin)
                            .Cast<dynamic>()
                            .Where(y => y.value != null)
                            .Select(y => (double) y.value)
                            .ToArray(),
                        OutValues = ((JArray) x.vout)
                            .Cast<dynamic>()
                            .Where(y => y.value != null)
                            .Select(y => (double) y.value)
                            .ToArray(),
                    })
                    .ToArray()
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(m_ExplorerUri, $"?txid={hash}");

        // Yiimp explorer can't show address stats?
        public override Uri CreateAddressUrl(string address)
            => null;

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(m_ExplorerUri, $"?hash={blockHash}");
     
        public override WalletBalance GetWalletBalance(string address)
        {
            throw new NotImplementedException();
        }

        public override BlockExplorerWalletOperation[] GetWalletOperations(string address, DateTime startDate)
        {
            throw new NotImplementedException();
        }
    }
}
