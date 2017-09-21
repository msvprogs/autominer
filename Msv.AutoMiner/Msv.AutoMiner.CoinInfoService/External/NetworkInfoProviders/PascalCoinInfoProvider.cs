using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    //API: http://explorer.pascalcoin.org/api.php
    public class PascalCoinInfoProvider : NetworkInfoProviderBase
    {
        private static readonly Uri M_BaseUri = new Uri("http://explorer.pascalcoin.org");

        private readonly IWebClient m_WebClient;

        public PascalCoinInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var height = long.Parse(m_WebClient.DownloadString(new Uri(M_BaseUri, "/api.php?blockcount")));
            var blocks = Enumerable.Range(1, 5)
                .Select(x => m_WebClient.DownloadString(new Uri(M_BaseUri, $"/api.php?block={height - x}")))
                .Select(JsonConvert.DeserializeObject)
                .Cast<dynamic>()
                .Select(x => new BlockInfo((long)x.timestamp, (long)x.block, (double)x.reward))
                .ToArray();
            var hashrate = long.Parse(
                m_WebClient.DownloadString(new Uri(M_BaseUri, "/api.php?hashrate"))) * 1000;
            var blockStats = CalculateBlockStats(blocks);
            if (blockStats == null)
                return new CoinNetworkStatistics {Height = height, NetHashRate = hashrate};
            return new CoinNetworkStatistics
            {
                BlockReward = blockStats.Value.LastReward,
                BlockTimeSeconds = blockStats.Value.MeanBlockTime,
                Height = height,
                NetHashRate = hashrate
            };
        }
    }
}
