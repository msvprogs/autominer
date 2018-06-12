using System;
using System.Linq;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    //API: https://explorer.zcha.in/api
    [SpecificCoinInfoProvider("ZEC")]
    public class ZcashInfoProvider : INetworkInfoProvider
    {
        private static readonly Uri M_BaseUri = new Uri("https://api.zcha.in/v2/mainnet/");
        private static readonly Uri M_ExplorerBaseUri = new Uri("https://explorer.zcha.in/");

        private readonly IWebClient m_WebClient;

        public ZcashInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic networkInfo = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_BaseUri, "network")));
            dynamic lastBlockInfo = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_BaseUri, "blocks/" + (string) networkInfo.blockHash)));
            var lastBlockTransactions = JsonConvert.DeserializeObject<JArray>(
                m_WebClient.DownloadString(new Uri(M_BaseUri,
                    $"blocks/{(string) networkInfo.blockHash}/transactions?limit=20&offset=0&sort=index&direction=ascending")));
            return new CoinNetworkStatistics
            {
                Difficulty = (double) networkInfo.difficulty,
                NetHashRate = (long) networkInfo.hashrate,
                BlockTimeSeconds = (double) networkInfo.meanBlockTime,
                Height = (long) networkInfo.blockNumber,
                LastBlockTime = DateTimeHelper.ToDateTimeUtc((long) lastBlockInfo.timestamp),
                LastBlockTransactions = lastBlockTransactions
                    .Cast<dynamic>()
                    .Select(x => new TransactionInfo
                    {
                        Fee = (double) x.fee,
                        InValues = (string) x.type == "minerReward"
                            ? new double[0]
                            : new double[] {x.value},
                        OutValues = new double[] {x.outputValue}
                    })
                    .ToArray()
            };
        }

        public WalletBalance GetWalletBalance(string address)
        {
            throw new NotImplementedException();
        }

        public BlockExplorerWalletOperation[] GetWalletOperations(string address, DateTime startDate)
        {
            throw new NotImplementedException();
        }

        public Uri CreateTransactionUrl(string hash)
            => new Uri(M_ExplorerBaseUri, $"transactions/{hash}");

        public Uri CreateAddressUrl(string address)
            => new Uri(M_ExplorerBaseUri, $"accounts/{address}");

        public Uri CreateBlockUrl(string blockHash)
            => new Uri(M_ExplorerBaseUri, $"blocks/{blockHash}");
    }
}
