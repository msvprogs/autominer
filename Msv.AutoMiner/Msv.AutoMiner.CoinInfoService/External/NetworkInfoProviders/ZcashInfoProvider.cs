using System;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    //API: https://explorer.zcha.in/api
    public class ZcashInfoProvider : INetworkInfoProvider
    {
        private static readonly Uri M_BaseUri = new Uri("https://api.zcha.in");

        private readonly IWebClient m_WebClient;

        public ZcashInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic networkInfo = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_BaseUri, "/v2/mainnet/network")));
            return new CoinNetworkStatistics
            {
                Difficulty = (double) networkInfo.difficulty,
                NetHashRate = (long) networkInfo.hashrate,
                BlockTimeSeconds = (double) networkInfo.meanBlockTime,
                Height = (long) networkInfo.blockNumber
            };
        }
    }
}
