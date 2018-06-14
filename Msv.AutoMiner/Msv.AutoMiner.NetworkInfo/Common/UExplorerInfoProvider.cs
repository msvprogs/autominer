using System;
using System.Linq;
using HtmlAgilityPack;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.NetworkInfo.Common
{
    public class UExplorerInfoProvider : NetworkInfoProviderBase
    {
        private const string LastBlockRequestUrl =
            "datatables/blocks?draw=1&columns%5B0%5D%5Bdata%5D=Height&columns%5B0%5D%5Bname%5D=Height&columns%5B0%5D%5Bsearchable%5D=true"
            + "&columns%5B0%5D%5Borderable%5D=false&columns%5B0%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B0%5D%5Bsearch%5D%5Bregex%5D=false"
            + "&columns%5B1%5D%5Bdata%5D=Time&columns%5B1%5D%5Bname%5D=Time&columns%5B1%5D%5Bsearchable%5D=true&columns%5B1%5D%5B"
            + "orderable%5D=false&columns%5B1%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B1%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B2%5D%5B"
            + "data%5D=TotalTx&columns%5B2%5D%5Bname%5D=TotalTx&columns%5B2%5D%5Bsearchable%5D=true&columns%5B2%5D%5Borderable%5D=false"
            + "&columns%5B2%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B2%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B3%5D%5Bdata%5D=Type&columns"
            + "%5B3%5D%5Bname%5D=Type&columns%5B3%5D%5Bsearchable%5D=true&columns%5B3%5D%5Borderable%5D=false&columns%5B3%5D%5Bsearch%5D%5B"
            + "value%5D=&columns%5B3%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B4%5D%5Bdata%5D=Reward&columns%5B4%5D%5Bname%5D=Reward&"
            + "columns%5B4%5D%5Bsearchable%5D=true&columns%5B4%5D%5Borderable%5D=false&columns%5B4%5D%5Bsearch%5D%5Bvalue%5D=&columns"
            + "%5B4%5D%5Bsearch%5D%5Bregex%5D=false&start=0&length=15&search%5Bvalue%5D=&search%5Bregex%5D=false";

        private const string LastTransactionsRequest =
            "datatables/transactions?draw=1&columns[0][data]=Transaction&columns[0][name]=Transaction&columns[0][searchable]=true" +
            "&columns[0][orderable]=false&columns[0][search][value]=&columns[0][search][regex]=false&columns[1][data]=Time" +
            "&columns[1][name]=Time&columns[1][searchable]=true&columns[1][orderable]=false&columns[1][search][value]=" +
            "&columns[1][search][regex]=false&columns[2][data]=Total&columns[2][name]=Total&columns[2][searchable]=true" +
            "&columns[2][orderable]=false&columns[2][search][value]=&columns[2][search][regex]=false&start=0&length=30" +
            "&search[value]=&search[regex]=false";

        private readonly IWebClient m_WebClient;
        private readonly Uri m_BaseUrl;

        public UExplorerInfoProvider(IWebClient webClient, string baseUrl)
        {
            if (baseUrl == null) 
                throw new ArgumentNullException(nameof(baseUrl));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_BaseUrl = new Uri(baseUrl);
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            dynamic stats = m_WebClient.DownloadJArray(new Uri(m_BaseUrl, "/api/chart/stat"))[0];
            var lastBlock = ((JArray)m_WebClient.DownloadJsonAsDynamic(
                    new Uri(m_BaseUrl, LastBlockRequestUrl)).data)
                .Cast<dynamic>()
                .First(x => (string)x.Type == "POW");

            var lastBlockHash = HtmlNode.CreateNode((string) lastBlock.Hash).InnerText;
            var lastBlockTransactions = ((JArray) m_WebClient.DownloadJsonAsDynamic(
                    new Uri(m_BaseUrl, LastTransactionsRequest)).data)
                .Cast<dynamic>()
                .Where(x => (string) x.Block == lastBlockHash)
                .Select(x => new TransactionInfo
                {
                    InValues = ((JArray) x.In)
                        .Cast<dynamic>()
                        .Where(y => (string) y.Transaction != "coinbase")
                        .Select(y => (double) y.Amount)
                        .ToArray(),
                    OutValues = ((JArray) x.Out)
                        .Cast<dynamic>()
                        .Select(y => (double) y.Amount)
                        .ToArray()
                })
                .ToArray();

            return new CoinNetworkStatistics
            {
                Difficulty = (double) stats.Difficulty,
                Height = (long) stats.Block,
                LastBlockTime = DateTimeHelper.FromIso8601((string) lastBlock.TimeNormal),
                MasternodeCount = (int) stats.Masternode,
                TotalSupply = (double) stats.Supply,
                NetHashRate = (double) stats.Network * 1e6,
                LastBlockTransactions = lastBlockTransactions
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
            => new Uri(m_BaseUrl, $"transactions/{hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(m_BaseUrl, $"wallets/{address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(m_BaseUrl, $"blocks/{blockHash}");
    }
}
