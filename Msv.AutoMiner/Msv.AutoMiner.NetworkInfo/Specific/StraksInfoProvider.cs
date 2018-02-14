using System;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Common;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    public class StraksInfoProvider : NetworkInfoProviderBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://straks.info/");

        private readonly IWebClient m_WebClient;

        public StraksInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public override CoinNetworkStatistics GetNetworkStats()
        {
            dynamic json = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString("https://api.straks.info/v2/statistics/latest"));
            return new CoinNetworkStatistics
            {
                NetHashRate = (double) json.hashrate,
                Difficulty = (double) json.difficulty,
                Height = (long) json.block_height,
                LastBlockTime = DateTimeHelper.ToDateTimeUtc((long) json.last_block),
                TotalSupply = (double) json.total_coins
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(M_BaseUri, $"transaction/{hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(M_BaseUri, $"address/{address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(M_BaseUri, $"block/{blockHash}");
    }
}
