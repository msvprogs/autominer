using System;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    //API: https://blockexplorer.com/api-ref
    //API2: https://blockchain.info/api/blockchain_api
    //API3: https://blockchain.info/api/charts_api
    public class BitCoinInfoProvider : NetworkInfoProviderBase
    {
        private readonly IWebClient m_WebClient;

        public BitCoinInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var blockRewardString = m_WebClient.DownloadString("https://blockchain.info/ru/q/bcperblock");
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
    }
}