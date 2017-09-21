using System;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    public class KomodoInfoProvider : InsightInfoProvider
    {
        private readonly IWebClient m_WebClient;

        public KomodoInfoProvider(IWebClient webClient)
            : base(webClient, "http://kmd.explorer.supernet.org/api")
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var stats = base.GetNetworkStats();

            dynamic hashRateJson = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString("http://www.komodopool.com/api/stats"));
            stats.NetHashRate = (long) ((double) hashRateJson.algos.equihash.hashrate / 1e4);
            return stats;
        }
    }
}
