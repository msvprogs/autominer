using System;
using System.Linq;
using Msv.AutoMiner.Service.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.Service.External.Network.Common
{
    public class IquidusInfoProvider : NetworkInfoProviderBase
    {
        private readonly Uri m_BaseUrl;

        public IquidusInfoProvider(string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(baseUrl));

            m_BaseUrl = new Uri(baseUrl);
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            dynamic stats = JsonConvert.DeserializeObject(
                DownloadString(new Uri(m_BaseUrl, "/ext/summary").ToString()));
            dynamic transactions = JsonConvert.DeserializeObject(
                DownloadString(new Uri(m_BaseUrl, GetTransactionUrl()).ToString()));
            var blockStats = CalculateBlockStats(
                ((JArray) transactions.data)
                .Cast<dynamic>()
                .Where(x => x.vin.Count > 0 && (string)x.vin[0].addresses == "coinbase")
                .Select(x => new BlockInfo
                {
                    Height = (long) x.blockindex,
                    Timestamp = (long) x.timestamp,
                    Reward = (double)x.vin[0].amount / 1e8
                })
                .Distinct());
            var hashRate = ParsingHelper.ParseHashRate((string) stats.data[0].hashrate);
            return new CoinNetworkStatistics
            {
                Difficulty = GetDifficulty(stats.data[0].difficulty),
                NetHashRate = (long)(hashRate * 1e9),
                Height = (long)stats.data[0].blockcount,
                BlockTimeSeconds = blockStats?.MeanBlockTime,
                BlockReward = blockStats?.LastReward
            };
        }

        protected virtual string GetTransactionUrl()
            => "/ext/getlasttxs/10/0.00000001";

        protected virtual double GetDifficulty(dynamic difficultyValue)
            => (double)difficultyValue;
    }
}
