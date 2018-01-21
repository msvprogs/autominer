using System;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.NetworkInfo.Common;
using Newtonsoft.Json;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    //API: https://blockexplorer.com/api-ref
    //API2: https://blockchain.info/api/blockchain_api
    //API3: https://blockchain.info/api/charts_api
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
                Height = (long)statsJson.n_blocks_total
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(M_BlockChainBaseUrl, $"tx/{hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(M_BlockChainBaseUrl, $"address/{address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(M_BlockChainBaseUrl, $"block/{blockHash}");
    }
}