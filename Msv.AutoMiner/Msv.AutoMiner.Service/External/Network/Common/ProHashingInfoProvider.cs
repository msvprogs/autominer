using System;
using System.Linq;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.Service.External.Network.Common
{
    public class ProHashingInfoProvider : WebDownloaderBase, ICoinNetworkInfoProvider
    {
        private readonly string m_CurrencyName;

        public ProHashingInfoProvider(string currencyName)
        {
            if (string.IsNullOrEmpty(currencyName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(currencyName));

            m_CurrencyName = currencyName;
        }

        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic stats = JsonConvert.DeserializeObject(
                DownloadString($"https://prohashing.com/explorerJson/getInfo?coin_name={m_CurrencyName}"));
            var lastBlockHash = (string) stats.last_hash;
            dynamic lastBlock = JsonConvert.DeserializeObject(
                DownloadString($"https://prohashing.com/explorerJson/getBlock?coin_id={stats.id}&hash={lastBlockHash}"));
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
