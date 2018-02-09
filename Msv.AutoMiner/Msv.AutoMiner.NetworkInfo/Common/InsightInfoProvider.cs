using System;
using System.Reflection;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;
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
            dynamic blockJson = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(m_BaseUrl + "/blocks?limit=1"));

            var difficulty = GetDifficulty(infoJson.info);
            return new CoinNetworkStatistics
            {
                Difficulty = difficulty,
                Height = (long)infoJson.info.blocks,
                LastBlockTime = DateTimeHelper.ToDateTimeUtc((long)blockJson.blocks[0].time)
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(m_BaseUrl, $"/tx/{hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(m_BaseUrl, $"/address/{address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(m_BaseUrl, $"/block/{blockHash}");

        [Obfuscation(Exclude = true)]
        protected virtual double GetDifficulty(dynamic statsInfo) 
            => (double) statsInfo.difficulty;
    }
}
