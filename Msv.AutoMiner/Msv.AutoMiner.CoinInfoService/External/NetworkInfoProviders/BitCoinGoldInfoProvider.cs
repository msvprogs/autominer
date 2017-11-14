using System;
using System.Globalization;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
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
            var lastBlocks = JsonConvert.DeserializeObject<JArray>(
                m_WebClient.DownloadString(new Uri(M_BaseUri, "/ext/getlastblocks")));
            var blockStats = CalculateBlockStats(lastBlocks
                .Cast<dynamic>()
                .Where(x => x.transactions.vin.Count > 0
                            && x.transactions.vin[0].coinbase != null
                            && ((JArray)x.transactions.vout).Count > 0
                            && (double)x.transactions.vout[0].value > 0)
                .Select(x =>
                    new BlockInfo((long)x.time, (long)x.height, (double)x.transactions.vout[0].value))
                .Distinct());
            return new CoinNetworkStatistics
            {
                Difficulty = (double)stats.data[0].difficulty,
                NetHashRate = double.TryParse(
                    (string)stats.data[0].hashrate, NumberStyles.Any, CultureInfo.InvariantCulture, out var hashRate)
                    ? hashRate * 1e6
                    : 0,
                Height = (long)stats.data[0].blockcount,
                BlockTimeSeconds = blockStats?.MeanBlockTime,
                BlockReward = blockStats?.LastReward
            };
        }
    }
}
