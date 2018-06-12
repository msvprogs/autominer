using System;
using System.Linq;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Common;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    //API: https://blockexplorer.com/api-ref
    //API2: https://blockchain.info/api/blockchain_api
    //API3: https://blockchain.info/api/charts_api
    [SpecificCoinInfoProvider("BTC")]
    public class BitCoinInfoProvider : NetworkInfoProviderBase
    {
        private static readonly Uri M_BlockChainBaseUrl = new Uri("https://blockchain.info/en/");

        private readonly IWebClient m_WebClient;

        public BitCoinInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var blockRewardString = m_WebClient.DownloadString(new Uri(M_BlockChainBaseUrl, "q/bcperblock"));
            dynamic statsJson = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString("https://api.blockchain.info/stats"));
            return new CoinNetworkStatistics
            {
                Difficulty = (double)statsJson.difficulty,
                BlockReward = double.Parse(blockRewardString) / 1e8,
                BlockTimeSeconds = (double)statsJson.minutes_between_blocks * 60,
                NetHashRate = (double)statsJson.hash_rate * 1e9,
                Height = (long)statsJson.n_blocks_total,
                LastBlockTime = DateTimeHelper.ToDateTimeUtcMsec((long)statsJson.timestamp)
            };
        }

        public override WalletBalance GetWalletBalance(string address)
        {
            dynamic balancesJson = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_BlockChainBaseUrl, $"balance?active={address}")));
            return new WalletBalance
            {
                Available = ((double?) balancesJson[address]?.final_balance / 1e8).GetValueOrDefault()
            };
        }

        public override BlockExplorerWalletOperation[] GetWalletOperations(string address, DateTime startDate)
        {
            JArray transactions = JsonConvert.DeserializeObject<dynamic>(
                m_WebClient.DownloadString(new Uri(M_BlockChainBaseUrl, $"rawaddr/{address}"))).txs;

            return transactions
                .Cast<dynamic>()
                .Select(x => new BlockExplorerWalletOperation
                {
                    DateTime = DateTimeHelper.ToDateTimeUtc((long) x.time),
                    Address = address,
                    Transaction = (string) x.hash,
                    Amount = ((JArray) x.inputs)
                             .Cast<dynamic>()
                             .Where(y => y.prev_out != null && (string) y.prev_out.addr == address)
                             .Select(y => -(double?) y.prev_out.value / 1e8)
                             .FirstOrDefault()
                             ?? ((JArray) x.@out)
                             .Cast<dynamic>()
                             .Where(y => (string) y.addr == address)
                             .Select(y => (double) y.value / 1e8)
                             .First()
                })
                .Where(x => x.DateTime > startDate)
                .ToArray();
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(M_BlockChainBaseUrl, $"tx/{hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(M_BlockChainBaseUrl, $"address/{address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(M_BlockChainBaseUrl, $"block/{blockHash}");
    }
}