using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common
{
    public class ProHashingInfoProvider : INetworkInfoProvider
    {
        private static readonly Uri M_BaseUri = new Uri("https://prohashing.com");

        private readonly IWebClient m_WebClient;
        private readonly string m_CurrencyName;

        public ProHashingInfoProvider(IWebClient webClient, string currencyName)
        {
            if (string.IsNullOrEmpty(currencyName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(currencyName));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_CurrencyName = currencyName;
        }

        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic stats = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_BaseUri, $"/explorerJson/getInfo?coin_name={m_CurrencyName}")));
            var lastBlockHash = (string) stats.last_hash;
            dynamic lastBlock = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_BaseUri, $"/explorerJson/getBlock?coin_id={stats.id}&hash={lastBlockHash}")));
            var reward = ((JArray) lastBlock.tx)
                .Cast<dynamic>()
                .Where(x => (bool) x.coinbase)
                .Sum(x => (double?) x.value);
            return new CoinNetworkStatistics
            {
                Difficulty = (double) stats.difficulty,
                Height = (long) stats.blocks,
                BlockReward = reward
            };
        }
    }
}
