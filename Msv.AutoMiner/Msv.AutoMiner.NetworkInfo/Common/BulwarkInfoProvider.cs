using System;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.NetworkInfo.Data;

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
            var overallInfo = m_WebClient.DownloadJsonAsDynamic(new Uri(m_BaseUrl, "/api/coin"));
            var lastBlockInfo = m_WebClient.DownloadJsonAsDynamic(
                new Uri(m_BaseUrl, $"/api/block/{(string) overallInfo.blocks}"));

            return new CoinNetworkStatistics
            {
                Difficulty = (double) overallInfo.diff,
                Height = (long) overallInfo.blocks,
                NetHashRate = (double) overallInfo.netHash,
                MasternodeCount = (int) overallInfo.mnsOn,
                LastBlockTime = (DateTime) lastBlockInfo.block.createdAt
            };
        }

        public override WalletBalance GetWalletBalance(string address)
        {
            throw new NotImplementedException();
        }

        public override BlockExplorerWalletOperation[] GetWalletOperations(string address, DateTime startDate)
        {
            throw new NotImplementedException();
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(m_BaseUrl, $"/#/tx/{hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(m_BaseUrl, $"/#/address/{address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(m_BaseUrl, $"/#/block/{blockHash}");
    }
}
