using System;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    //API: https://www.feathercoin.com/feathercoin-api/
    public class FeatherCoinInfoProvider : INetworkInfoProvider
    {
        private static readonly Uri M_ExplorerBaseUri = new Uri("http://explorer.feathercoin.com");

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

        public Uri CreateTransactionUrl(string hash)
            => new Uri(M_ExplorerBaseUri, $"tx/{hash}");

        public Uri CreateAddressUrl(string address)
            => new Uri(M_ExplorerBaseUri, $"address/{address}");

        public Uri CreateBlockUrl(string blockHash)
            => new Uri(M_ExplorerBaseUri, $"block/{blockHash}");
    }
}
