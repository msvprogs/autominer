using System;
using System.Linq;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Common;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    [SpecificCoinInfoProvider("STAK")]
    public class StraksInfoProvider : NetworkInfoProviderBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://straks.info/");
        private static readonly Uri M_ApiBaseUri = new Uri("https://api.straks.info/v2/");

        private readonly IWebClient m_WebClient;

        public StraksInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public override CoinNetworkStatistics GetNetworkStats()
        {
            dynamic statsJson = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_ApiBaseUri, "statistics/latest")));
            dynamic bestBlockHashJson = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_ApiBaseUri, $"block-index/{statsJson.block_height}")));
            dynamic bestBlockTransactions = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_ApiBaseUri, 
                    $"txs?block={bestBlockHashJson.blockhash}&pageNum=1&limit=50")));

            return new CoinNetworkStatistics
            {
                NetHashRate = (double) statsJson.hashrate,
                Difficulty = (double) statsJson.difficulty,
                Height = (long) statsJson.block_height,
                LastBlockTime = DateTimeHelper.ToDateTimeUtc((long) statsJson.last_block),
                TotalSupply = (double) statsJson.total_coins,
                LastBlockTransactions = ((JArray)bestBlockTransactions.txs)
                    .Cast<dynamic>()
                    .Select(x => new TransactionInfo
                    {
                        InValues = ((JArray)x.vin)
                            .Cast<dynamic>()
                            .Where(y => y.value != null)
                            .Select(y => (double)y.value)
                            .ToArray(),
                        OutValues = ((JArray)x.vout)
                            .Cast<dynamic>()
                            .Where(y => y.value != null)
                            .Select(y => (double)y.value)
                            .ToArray()
                    })
                    .ToArray()
            };
        }

        public override WalletBalance GetWalletBalance(string address)
        {
            throw new NotImplementedException();
        }

        public override BlockExplorerWalletOperation[] GetWalletOperations(string address, DateTime startDate)
        {
            throw new NotImplementedException();
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(M_BaseUri, $"transaction/{hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(M_BaseUri, $"address/{address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(M_BaseUri, $"block/{blockHash}");
    }
}
