using System;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    //API: https://www.etherchain.org/documentation/api
    public class EthereumInfoProvider : INetworkInfoProvider
    {
        private static readonly Uri M_EtherchainBaseUri = new Uri("https://www.etherchain.org");

        private readonly IWebClient m_WebClient;

        public EthereumInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic statsJson = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_EtherchainBaseUri, "/api/miningEstimator")));
            dynamic heightJson = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_EtherchainBaseUri, "/api/blocks/count")));
            return new CoinNetworkStatistics
            {
                Difficulty = (double) statsJson.difficulty,
                NetHashRate = (double) statsJson.hashrate,
                BlockTimeSeconds = (double) statsJson.blocktime,
                Height = (long) heightJson.count
            };
        }
    }
}
