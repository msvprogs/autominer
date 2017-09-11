using System;
using Msv.AutoMiner.Service.Data;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Service.External.Network.Common
{
    public class SwaggerInfoProvider : NetworkInfoProviderBase
    {
        private readonly Uri m_BaseUrl;

        public SwaggerInfoProvider(string baseUrl)
        {
            if (baseUrl == null)
                throw new ArgumentNullException(nameof(baseUrl));

            m_BaseUrl = new Uri(baseUrl);
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            dynamic stats = JsonConvert.DeserializeObject(DownloadString(new Uri(m_BaseUrl, "/Blockchain/GetMiningInfo").ToString()));
            return new CoinNetworkStatistics
            {
                Height = (long?) stats.result.blocks,
                Difficulty = (double) stats.result.difficulty,
                NetHashRate = (long) stats.result.networkhashps
            };
        }
    }
}
