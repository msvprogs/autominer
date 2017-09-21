using System;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    public class DecredInfoProvider : INetworkInfoProvider
    {
        private readonly IWebClient m_WebClient;

        public DecredInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic json = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString("https://dcrstats.com/api/v1/get_stats"));
            return new CoinNetworkStatistics
            {
                Difficulty = (double) json.difficulty,
                BlockReward = (double) json.pow_reward,
                BlockTimeSeconds = (double) json.average_time,
                NetHashRate = (long) json.networkhashps,
                Height = (long) json.blocks
            };
        }
    }
}
