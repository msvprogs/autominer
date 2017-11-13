using System;
using System.Globalization;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common
{
    public class IquidusInfoProvider : NetworkInfoProviderBase
    {
        private readonly IWebClient m_WebClient;
        private readonly Uri m_BaseUrl;

        public IquidusInfoProvider(IWebClient webClient, string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(baseUrl));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_BaseUrl = new Uri(baseUrl);
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            dynamic stats = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(m_BaseUrl, "/ext/summary")));
            dynamic transactions = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(m_BaseUrl, GetTransactionUrl())));
            var blockStats = CalculateBlockStats(
                ((JArray) transactions.data)
                .Cast<dynamic>()
                .Where(x => x.vin.Count > 0 && (string) x.vin[0].addresses == "coinbase" && ((JArray) x.vout).Count > 0)
                .Select(x => new BlockInfo((long) x.timestamp, (long) x.blockindex, (double) x.vout[0].amount / 1e8))
                .Distinct());
            return new CoinNetworkStatistics
            {
                Difficulty = GetDifficulty(stats.data[0].difficulty),
                NetHashRate = double.TryParse(
                    (string) stats.data[0].hashrate, NumberStyles.Any, CultureInfo.InvariantCulture, out var hashRate)
                    ? GetRealHashRate(hashRate)
                    : 0,
                Height = (long) stats.data[0].blockcount,
                BlockTimeSeconds = blockStats?.MeanBlockTime,
                BlockReward = blockStats?.LastReward
            };
        }

        protected virtual string GetTransactionUrl()
            => "/ext/getlasttxs/10/0.00000001";

        protected virtual double GetDifficulty(dynamic difficultyValue)
            => (double)difficultyValue;

        private double GetRealHashRate(double hashRate)
        {
            switch (m_BaseUrl.Host.ToLowerInvariant())
            {
                case "btczexplorer.blockhub.info":
                    return hashRate * 1e3;
                case "btgexp.com":
                    return hashRate * 1e6;
                default:
                    return hashRate * 1e9;
            }
        }
    }
}
