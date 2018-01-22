using System;
using System.Globalization;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Common;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    public class BitCoinGoldInfoProvider : NetworkInfoProviderBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://btgexp.com");

        private readonly IWebClient m_WebClient;

        public BitCoinGoldInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public override CoinNetworkStatistics GetNetworkStats()
        {
            dynamic stats = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_BaseUri, "/ext/summary")));

            var height = (long) stats.data[0].blockcount;
            var lastBlockHash = m_WebClient.DownloadString(
                new Uri(M_BaseUri, "/api/getblockhash?index=" + height));
            dynamic lastBlockInfo = JsonConvert.DeserializeObject(m_WebClient.DownloadString(
                new Uri(M_BaseUri, "/api/getblock?hash=" + lastBlockHash)));

            return new CoinNetworkStatistics
            {
                Difficulty = (double) stats.data[0].difficulty,
                NetHashRate = double.TryParse(
                    (string) stats.data[0].hashrate, NumberStyles.Any, CultureInfo.InvariantCulture, out var hashRate)
                    ? hashRate * 1e6
                    : 0,
                Height = height,
                LastBlockTime = DateTimeHelper.ToDateTimeUtc((long)lastBlockInfo.time)
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(M_BaseUri, $"/tx/{hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(M_BaseUri, $"/address/{address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(M_BaseUri, $"/block/{blockHash}");
    }
}
