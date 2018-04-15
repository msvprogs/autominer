using System;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;

namespace Msv.AutoMiner.NetworkInfo.Common
{
    public class BulwarkInfoProvider : NetworkInfoProviderBase
    {
        private readonly IWebClient m_WebClient;
        private readonly Uri m_BaseUrl;

        public BulwarkInfoProvider(IWebClient webClient, string baseUrl)
        {
            if (baseUrl == null) 
                throw new ArgumentNullException(nameof(baseUrl));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_BaseUrl = new Uri(baseUrl);
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            dynamic overallInfo = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(m_BaseUrl, "/api/coin")));
            dynamic lastBlockInfo = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(m_BaseUrl, $"/api/block/{(string) overallInfo.blocks}")));

            return new CoinNetworkStatistics
            {
                Difficulty = (double) overallInfo.diff,
                Height = (long) overallInfo.blocks,
                NetHashRate = (double) overallInfo.netHash,
                MasternodeCount = (int) overallInfo.mnsOn,
                LastBlockTime = (DateTime) lastBlockInfo.block.createdAt
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(m_BaseUrl, $"/#/tx/{hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(m_BaseUrl, $"/#/address/{address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(m_BaseUrl, $"/#/block/{blockHash}");
    }
}
