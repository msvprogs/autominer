using System;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;

namespace Msv.AutoMiner.NetworkInfo.Common
{
    public class EtcExplorerInfoProvider : NetworkInfoProviderBase
    {
        private readonly IWebClient m_WebClient;
        private readonly Uri m_BaseUrl;

        public EtcExplorerInfoProvider(IWebClient webClient, string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(baseUrl));
            
            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_BaseUrl = new Uri(baseUrl);
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var blocks = DoPostRequest("/data", new {action = "latest_blocks"});
            long lastBlockNum = blocks.blocks[0].number;
            var lastBlockTime = DateTimeHelper.ToDateTimeUtc((long) blocks.blocks[0].timestamp);

            var lastBlockInfo = DoPostRequest("/web3relay", new {block = lastBlockNum});

            return new CoinNetworkStatistics
            {
                Difficulty = (double) lastBlockInfo.difficulty,
                Height = lastBlockNum,
                LastBlockTime = lastBlockTime,
            };

            dynamic DoPostRequest(string url, object request) 
                => JsonConvert.DeserializeObject(m_WebClient.UploadString(
                    new Uri(m_BaseUrl, url).ToString(), JsonConvert.SerializeObject(request), null,
                    contentType: "application/json"));
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
            => new Uri(m_BaseUrl, $"/tx/{hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(m_BaseUrl, $"/addr/{address}");

        // Blocks are searched only by height
        public override Uri CreateBlockUrl(string blockHash)
            => null;
    }
}
