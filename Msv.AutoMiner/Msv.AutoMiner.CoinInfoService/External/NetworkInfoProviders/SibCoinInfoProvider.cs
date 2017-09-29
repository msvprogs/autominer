using System;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    public class SibCoinInfoProvider : NetworkInfoProviderBase
    {
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
    }
}
