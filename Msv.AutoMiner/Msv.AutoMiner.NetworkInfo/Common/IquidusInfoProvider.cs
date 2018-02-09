using System;
using System.Globalization;
using System.Reflection;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;
using Msv.AutoMiner.NetworkInfo.Utilities;
using Newtonsoft.Json;

namespace Msv.AutoMiner.NetworkInfo.Common
{
    public class IquidusInfoProvider : NetworkInfoProviderBase
    {
        private readonly IWebClient m_WebClient;
        private readonly NetworkInfoProviderOptions m_Options;
        private readonly Uri m_BaseUrl;

        public IquidusInfoProvider(IWebClient webClient, string baseUrl, NetworkInfoProviderOptions options)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(baseUrl));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_Options = options ?? throw new ArgumentNullException(nameof(options));
            m_BaseUrl = new Uri(baseUrl);
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            dynamic stats = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(m_BaseUrl, "/ext/summary")));

            var height = (long) stats.data[0].blockcount;
            var lastBlockHash = m_WebClient.DownloadString(
                new Uri(m_BaseUrl, "/api/getblockhash?index=" + height));
            var lastBlockInfo = JsonConvert.DeserializeObject<BlockHeader>(m_WebClient.DownloadString(
                new Uri(m_BaseUrl, "/api/getblock?hash=" + lastBlockHash)));

            var difficulty = m_Options.GetDifficultyFromLastPoWBlock
                ? new BlockChainSearcher(x => JsonConvert.DeserializeObject<BlockHeader>(
                        m_WebClient.DownloadString(new Uri(m_BaseUrl, "/api/getblock?hash=" + x))))
                    .SearchPoWBlock(lastBlockInfo).Difficulty
                : GetDifficulty(stats.data[0].difficulty);
            return new CoinNetworkStatistics
            {
                Difficulty = difficulty,
                NetHashRate = double.TryParse(
                    (string) stats.data[0].hashrate, NumberStyles.Any, CultureInfo.InvariantCulture, out var hashRate)
                    ? GetRealHashRate(hashRate)
                    : 0,
                Height = height,
                LastBlockTime = DateTimeHelper.ToDateTimeUtc(lastBlockInfo.Time)
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(m_BaseUrl, $"tx/{hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(m_BaseUrl, $"address/{address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(m_BaseUrl, $"block/{blockHash}");

        [Obfuscation(Exclude = true)]
        protected virtual double GetDifficulty(dynamic difficultyValue)
            => (double)difficultyValue;

        [Obfuscation(Exclude = true)]
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
