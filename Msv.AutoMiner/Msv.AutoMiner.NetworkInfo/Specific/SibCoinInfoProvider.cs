using System;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.NetworkInfo.Common;
using Newtonsoft.Json;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    public class SibCoinInfoProvider : NetworkInfoProviderBase
    {
        private static readonly Uri M_ExplorerBaseUrl = new Uri("https://chain.sibcoin.net/en/");

        private readonly IWebClient m_WebClient;

        public SibCoinInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public override CoinNetworkStatistics GetNetworkStats()
        {
            dynamic stats = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString("http://sibinform.su/json/index_stat.json"));
            return new CoinNetworkStatistics
            {
                Difficulty = (double) stats.last_difficulty,
                NetHashRate = (long) (stats.nethash * 1e9),
                BlockTimeSeconds = (double) stats.block_generate_time * 60,
                BlockReward = (double) stats.block_reward_miner,
                Height = (long) stats.blocks
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(M_ExplorerBaseUrl, $"tx/{hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(M_ExplorerBaseUrl, $"address/{address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(M_ExplorerBaseUrl, $"block/{blockHash}");
    }
}
