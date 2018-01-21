using System;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.NetworkInfo.Common
{
    public class InsightInfoProvider : NetworkInfoProviderBase
    {
        private readonly IWebClient m_WebClient;
        private readonly Uri m_BaseUrl;

        public InsightInfoProvider(IWebClient webClient, string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(baseUrl));
            
            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_BaseUrl = new Uri(baseUrl);
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            dynamic infoJson = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(m_BaseUrl + "/status?q=getInfo"));

            return new CoinNetworkStatistics
            {
                Difficulty = GetDifficulty(infoJson.info),
                Height = (long)infoJson.info.blocks
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(m_BaseUrl, $"tx/{hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(m_BaseUrl, $"address/{address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(m_BaseUrl, $"block/{blockHash}");

        protected virtual double GetDifficulty(dynamic statsInfo) 
            => (double) statsInfo.difficulty;
    }
}
