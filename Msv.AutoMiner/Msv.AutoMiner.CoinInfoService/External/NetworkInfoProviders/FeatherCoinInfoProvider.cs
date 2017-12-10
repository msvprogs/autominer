using System;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    //API: https://www.feathercoin.com/feathercoin-api/
    public class FeatherCoinInfoProvider : INetworkInfoProvider
    {
        private readonly IWebClient m_WebClient;

        public FeatherCoinInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic stats = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString("http://api.feathercoin.com/?output=stats"));
            var height = (long) stats.currblk;
            return new CoinNetworkStatistics
            {
                Difficulty = (double) stats.nowdiff,
                NetHashRate = (long) ((double) stats.khs * 1000),
                BlockTimeSeconds = (double)stats.exptimeperblk,
                Height = height
            };
        }
    }
}
