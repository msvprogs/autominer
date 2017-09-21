using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            var transactions = JsonConvert.DeserializeObject<JArray>(m_WebClient.DownloadString(
                new Uri(M_BaseUri, "/v2/mainnet/transactions?sort=blockHeight&direction=descending&limit=20&offset=0")));
            var rewardTx = transactions
                .Cast<dynamic>()
                .First(x => (bool) x.mainChain && (string) x.type == "minerReward");
            return new CoinNetworkStatistics
            {
                Difficulty = (double) networkInfo.difficulty,
                NetHashRate = (long) networkInfo.hashrate,
                BlockTimeSeconds = (double) networkInfo.meanBlockTime,
                Height = (long) networkInfo.blockNumber,
                BlockReward = ((JArray)rewardTx.vout).Cast<dynamic>().Sum(x => (double)x.value)
            };
        }
    }
}
