using System;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Service.External.Network.Common
{
    public class OpenEthereumPoolInfoProvider : WebDownloaderBase, ICoinNetworkInfoProvider
    {
        private readonly string m_StatsUrl;

        public OpenEthereumPoolInfoProvider(string statsUrl)
        {
            if (string.IsNullOrEmpty(statsUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(statsUrl));

            m_StatsUrl = statsUrl;
        }

        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic json = JsonConvert.DeserializeObject(DownloadString(m_StatsUrl));
            var height = (long) json.nodes[0].height;
            return new CoinNetworkStatistics
            {
                Difficulty = (double)json.nodes[0].difficulty,
                Height = height,
                NetHashRate = 0
            };
        }
    }
}
