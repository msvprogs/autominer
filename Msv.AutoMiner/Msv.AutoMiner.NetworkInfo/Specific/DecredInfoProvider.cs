using System;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    [SpecificCoinInfoProvider("DCR")]
    public class DecredInfoProvider : INetworkInfoProvider
    {
        private static readonly Uri M_ExplorerBaseUri = new Uri("https://mainnet.decred.org/");

        private readonly IWebClient m_WebClient;

        public DecredInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic json = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString("https://dcrstats.com/api/v1/get_stats"));
            return new CoinNetworkStatistics
            {
                Difficulty = (double) json.difficulty,
                BlockReward = (double) json.pow_reward,
                BlockTimeSeconds = (double) json.average_time,
                NetHashRate = (long) json.networkhashps,
                Height = (long) json.blocks,
                LastBlockTime = DateTimeHelper.ToDateTimeUtc((long) json.last_block_datetime)
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
            => new Uri(M_ExplorerBaseUri, $"address/{address}");

        public Uri CreateBlockUrl(string blockHash)
            => new Uri(M_ExplorerBaseUri, $"block/{blockHash}");
    }
}
