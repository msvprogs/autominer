using System;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    public class PrimeCoinInfoProvider : INetworkInfoProvider
    {
        private readonly IWebClient m_WebClient;

        public PrimeCoinInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic json = JsonConvert.DeserializeObject(m_WebClient.DownloadString(
                "http://xpm.muuttuja.org/calc/current_block.json"));
            var difficulty = (double) json.difficulty;
            return new CoinNetworkStatistics
            {
                Difficulty = difficulty,
                BlockReward = Math.Floor(99900 / (difficulty * difficulty)) / 100,
                Height = (long) json.height
            };
        }
    }
}
