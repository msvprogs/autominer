using System;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common
{
    public abstract class SwaggerInfoProviderBase : NetworkInfoProviderBase
    {
        private readonly IWebClient m_WebClient;
        private readonly Uri m_BaseUrl;

        protected SwaggerInfoProviderBase(IWebClient webClient, string baseUrl)
        {
            if (baseUrl == null)
                throw new ArgumentNullException(nameof(baseUrl));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_BaseUrl = new Uri(baseUrl);
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            dynamic stats = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(m_BaseUrl, "/Blockchain/GetMiningInfo")));
            var height = (long) stats.result.blocks;
            return new CoinNetworkStatistics
            {
                Height = height,
                BlockReward = GetBlockReward(height),
                Difficulty = (double) stats.result.difficulty,
                NetHashRate = (long) stats.result.networkhashps
            };
        }

        protected abstract double GetBlockReward(long height);
    }
}
