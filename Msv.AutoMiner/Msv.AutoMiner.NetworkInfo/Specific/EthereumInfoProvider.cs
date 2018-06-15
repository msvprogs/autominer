using System;
using System.Linq;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    //API: https://www.blockcypher.com/dev/ethereum
    [SpecificCoinInfoProvider("ETH")]
    public class EthereumInfoProvider : INetworkInfoProvider
    {
        private const double WeisInEth = 1e18;

        private static readonly Uri M_EtherchainBaseUri = new Uri("https://www.etherchain.org");

        private readonly IWebClient m_WebClient;

        public EthereumInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public CoinNetworkStatistics GetNetworkStats()
        {
            var chainStats = m_WebClient.DownloadJsonAsDynamic("https://api.blockcypher.com/v1/eth/main");
            var rewardStats = m_WebClient.DownloadJsonAsDynamic(
                $"https://api.etherscan.io/api?module=block&action=getblockreward&blockno={chainStats.height}");
            var difficultyStats = m_WebClient.DownloadJsonAsDynamic("https://api.nanopool.org/v1/eth/block_stats/0/1");

            return new CoinNetworkStatistics
            {
                Height = (long)chainStats.height,
                LastBlockTime = ((DateTimeOffset)chainStats.time).UtcDateTime,
                BlockReward = ((double)rewardStats.result.blockReward - (double)rewardStats.result.uncleInclusionReward) / WeisInEth,
                Difficulty = (double)difficultyStats.data[0].difficulty
            };
        }

        public WalletBalance GetWalletBalance(string address)
        {
            var balanceInfo = m_WebClient.DownloadJsonAsDynamic(
                $"https://api.blockcypher.com/v1/eth/main/addrs/{address}/balance");

            return new WalletBalance
            {
                Available = (double) balanceInfo.balance / WeisInEth,
                Unconfirmed = (double) balanceInfo.unconfirmed_balance / WeisInEth
            };
        }

        public BlockExplorerWalletOperation[] GetWalletOperations(string address, DateTime startDate)
        {
            var transactionsInfo = m_WebClient.DownloadJsonAsDynamic(
                $"https://api.blockcypher.com/v1/eth/main/addrs/{address}");

            return ((JArray) transactionsInfo.txrefs)
                .Cast<dynamic>()
                .Where(x => x.confirmed != null)
                .Select(x => new BlockExplorerWalletOperation
                {
                    Transaction = (string) x.tx_hash,
                    DateTime = ((DateTimeOffset) x.confirmed).UtcDateTime,
                    Amount = (int) x.tx_output_n != 0
                        ? -(double) x.value / WeisInEth
                        : (double) x.value / WeisInEth,
                    Address = address
                })
                .Where(x => x.DateTime > startDate)
                .ToArray();
        }

        public Uri CreateTransactionUrl(string hash)
            => new Uri(M_EtherchainBaseUri, $"tx/{hash}");

        public Uri CreateAddressUrl(string address)
            => new Uri(M_EtherchainBaseUri, $"account/{address}");

        public Uri CreateBlockUrl(string blockHash)
            => new Uri(M_EtherchainBaseUri, $"block/{blockHash}");
    }
}