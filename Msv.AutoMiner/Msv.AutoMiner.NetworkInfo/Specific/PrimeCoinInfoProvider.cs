using System;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    [SpecificCoinInfoProvider("XPM")]
    public class PrimeCoinInfoProvider : INetworkInfoProvider
    {
        private static readonly Uri M_ExplorerBaseUri = new Uri("https://bchain.info/XPM/");

        private readonly IWebClient m_WebClient;

        public PrimeCoinInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic json = JsonConvert.DeserializeObject(m_WebClient.DownloadString(
                "http://xpm.muuttuja.org/calc/current_block.json"));
            var difficulty = (double) json.difficulty;
            return new CoinNetworkStatistics
            {
                Difficulty = difficulty,
                BlockReward = Math.Floor(99900 / (difficulty * difficulty)) / 100,
                Height = (long) json.height,
                LastBlockTime = DateTimeHelper.ToDateTimeUtc((long)json.time)
            };
        }

        public WalletBalance GetWalletBalance(string address)
        {
            throw new NotImplementedException();
        }

        public BlockExplorerWalletOperation[] GetWalletOperations(string address, DateTime startDate)
        {
            throw new NotImplementedException();
        }

        public Uri CreateTransactionUrl(string hash)
            => new Uri(M_ExplorerBaseUri, $"tx/{hash}");

        public Uri CreateAddressUrl(string address)
            => new Uri(M_ExplorerBaseUri, $"addr/{address}");

        public Uri CreateBlockUrl(string blockHash)
            => new Uri(M_ExplorerBaseUri, $"block/{blockHash}");
    }
}
