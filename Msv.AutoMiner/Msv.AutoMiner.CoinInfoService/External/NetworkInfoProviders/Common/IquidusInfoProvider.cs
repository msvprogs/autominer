using System;
using System.Globalization;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

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

            return new CoinNetworkStatistics
            {
                Difficulty = GetDifficulty(stats.data[0].difficulty),
                NetHashRate = double.TryParse(
                    (string) stats.data[0].hashrate, NumberStyles.Any, CultureInfo.InvariantCulture, out var hashRate)
                    ? GetRealHashRate(hashRate)
                    : 0,
                Height = (long) stats.data[0].blockcount
            };
        }

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
