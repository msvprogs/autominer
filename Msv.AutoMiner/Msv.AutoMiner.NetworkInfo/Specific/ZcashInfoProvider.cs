using System;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    //API: https://explorer.zcha.in/api
    public class ZcashInfoProvider : INetworkInfoProvider
    {
        private static readonly Uri M_BaseUri = new Uri("https://api.zcha.in");
        private static readonly Uri M_ExplorerBaseUri = new Uri("https://explorer.zcha.in/");

        private readonly IWebClient m_WebClient;

        public ZcashInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic networkInfo = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_BaseUri, "/v2/mainnet/network")));
            dynamic lastBlockInfo = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_BaseUri, "/v2/mainnet/blocks/" + (string) networkInfo.blockHash)));
            return new CoinNetworkStatistics
            {
                Difficulty = (double) networkInfo.difficulty,
                NetHashRate = (long) networkInfo.hashrate,
                BlockTimeSeconds = (double) networkInfo.meanBlockTime,
                Height = (long) networkInfo.blockNumber,
                LastBlockTime = DateTimeHelper.ToDateTimeUtc((long)lastBlockInfo.timestamp)
            };
        }
        
        public Uri CreateTransactionUrl(string hash)
            => new Uri(M_ExplorerBaseUri, $"transactions/{hash}");

        public Uri CreateAddressUrl(string address)
            => new Uri(M_ExplorerBaseUri, $"accounts/{address}");

        public Uri CreateBlockUrl(string blockHash)
            => new Uri(M_ExplorerBaseUri, $"blocks/{blockHash}");
    }
}
