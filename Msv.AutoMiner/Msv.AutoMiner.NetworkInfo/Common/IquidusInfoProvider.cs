using System;
using System.Globalization;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;

namespace Msv.AutoMiner.NetworkInfo.Common
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

            var height = (long) stats.data[0].blockcount;
            var lastBlockHash = m_WebClient.DownloadString(
                new Uri(m_BaseUrl, "/api/getblockhash?index=" + height));
            dynamic lastBlockInfo = JsonConvert.DeserializeObject(m_WebClient.DownloadString(
                new Uri(m_BaseUrl, "/api/getblock?hash=" + lastBlockHash)));

            return new CoinNetworkStatistics
            {
                Difficulty = GetDifficulty(stats.data[0].difficulty),
                NetHashRate = double.TryParse(
                    (string) stats.data[0].hashrate, NumberStyles.Any, CultureInfo.InvariantCulture, out var hashRate)
                    ? GetRealHashRate(hashRate)
                    : 0,
                Height = height,
                LastBlockTime = DateTimeHelper.ToDateTimeUtc((long)lastBlockInfo.time)
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(m_BaseUrl, $"tx/{hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(m_BaseUrl, $"address/{address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(m_BaseUrl, $"block/{blockHash}");

        protected virtual double GetDifficulty(dynamic difficultyValue)
            => (double)difficultyValue;

        private double GetRealHashRate(double hashRate)
        {
            switch (m_BaseUrl.Host.ToLowerInvariant())
            {
                case "btczexplorer.blockhub.info":
                    return hashRate * 1e3;
                default:
                    return hashRate * 1e9;
            }
        }
    }
}
