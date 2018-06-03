using System;
using System.Linq;
using System.Net;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;
using Msv.HttpTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.NetworkInfo.Common
{
    //API: https://chainz.cryptoid.info/api.dws
    public class ChainzCryptoidInfoProvider : NetworkInfoProviderBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://chainz.cryptoid.info");

        private readonly IProxiedWebClient m_WebClient;
        private readonly string m_CurrencySymbol;

        public ChainzCryptoidInfoProvider(IProxiedWebClient webClient, string currencySymbol)
        {
            if (string.IsNullOrEmpty(currencySymbol))
                throw new ArgumentException("Value cannot be null or empty.", nameof(currencySymbol));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_CurrencySymbol = currencySymbol.ToLowerInvariant();
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var blocksUri = new Uri(M_BaseUri, $"/explorer/index.data.dws?coin={m_CurrencySymbol}&n=20");
            string blocksJsonString;
            try
            {
                blocksJsonString = m_WebClient.DownloadString(blocksUri);
            }
            catch (CorrectHttpException ex) when (ex.Status == (HttpStatusCode) 429)
            {
                blocksJsonString = m_WebClient.DownloadStringProxied(blocksUri);
            }

            var blocks = ((JArray)JsonConvert.DeserializeObject<dynamic>(blocksJsonString).blocks)
                .Cast<dynamic>()
                .Where(x => (int)x.miner_id != 0) //not PoS
                .Select(x => new
                {
                    Height = (long)x.height,
                    Difficulty = (double)x.diff,
                    Timestamp = (long)x.dt
                })
                .ToArray();
            var height = blocks.Max(x => x.Height);

            var bestBlockHash = m_WebClient.DownloadString(new Uri(M_BaseUri,
                $"/{m_CurrencySymbol}/api.dws?q=getblockhash&height={height}")).Replace("\"", "");
            var bestBlockTransactions = JsonConvert.DeserializeObject<JArray>(m_WebClient.DownloadString(
                new Uri(M_BaseUri, $"/explorer/block.txs.dws?coin={m_CurrencySymbol}&h={bestBlockHash}.js")));

            var bestBlock = blocks.First(x => x.Height == height);
            return new CoinNetworkStatistics
            {
                Difficulty = bestBlock.Difficulty,
                Height = height,
                LastBlockTime = DateTimeHelper.ToDateTimeUtc(bestBlock.Timestamp),
                LastBlockTransactions = bestBlockTransactions
                    .Cast<dynamic>()
                    .Select(x => new TransactionInfo
                    {
                        InValues = ((JArray)x.inputs)
                            .Cast<dynamic>()
                            .Where(y => y is JObject && y.v != null)
                            .Select(y => (double)y.v)
                            .ToArray(),
                        OutValues = ((JArray)x.outputs)
                            .Cast<dynamic>()
                            .Where(y => y.v != null)
                            .Select(y => (double)y.v)
                            .ToArray()
                    })
                    .ToArray()
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(CreateCurrencyBaseUrl(), $"tx.dws?{hash}.htm");

        public override Uri CreateAddressUrl(string address)
            => new Uri(CreateCurrencyBaseUrl(), $"address.dws?{address}.htm");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(CreateCurrencyBaseUrl(), $"block.dws?{blockHash}.htm");

        private Uri CreateCurrencyBaseUrl()
            => new Uri(M_BaseUri, $"/{m_CurrencySymbol}/");
    }
}
