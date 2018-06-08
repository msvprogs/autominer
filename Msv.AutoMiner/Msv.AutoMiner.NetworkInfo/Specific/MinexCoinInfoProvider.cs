using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    [SpecificCoinInfoProvider("MNX")]
    public class MinexCoinInfoProvider : INetworkInfoProvider
    {
        // https://minexexplorer.com
        private static readonly Uri M_BaseUri = new Uri("http://0s.nvuw4zlymv4ha3dpojsxeltdn5wq.cmle.ru/api/");

        private readonly IWebClient m_WebClient;

        public MinexCoinInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic lastBlock = JsonConvert.DeserializeObject<JArray>(
                m_WebClient.DownloadString(new Uri(M_BaseUri, "block?minConfirmations=0")))[0];

            // Paginate transactions until current block coinbase is found
            var lastBlockTransactions = new List<TransactionInfo>();
            var page = 1;
            while (lastBlockTransactions.All(x => x.InValues.Any()) && page < 5)
                lastBlockTransactions.AddRange(JsonConvert.DeserializeObject<JArray>(
                        m_WebClient.DownloadString(new Uri(M_BaseUri, $"transaction?page={page++}")))
                    .Cast<dynamic>()
                    .Select(x => x.transaction)
                    .Where(x => (string) x.block == (string) lastBlock.hash)
                    .Select(x => new TransactionInfo
                    {
                        Fee = (double) x.fee,
                        InValues = ((JArray) x.inputs)
                            .Cast<dynamic>()
                            .Where(y => (string) y.address != "coinbase")
                            .Select(y => (double) y.amount)
                            .ToArray(),
                        OutValues = ((JArray) x.outputs)
                            .Cast<dynamic>()
                            .Select(y => (double) y.amount)
                            .ToArray()
                    }));

            return new CoinNetworkStatistics
            {
                Height = (long)lastBlock.height,
                Difficulty = (double)lastBlock.difficulty,
                NetHashRate = (double)lastBlock.hashRate,
                LastBlockTime = DateTimeHelper.FromIso8601((string)lastBlock.createdAt),
                LastBlockTransactions = lastBlockTransactions.ToArray()
            };
        }

        public Uri CreateTransactionUrl(string hash)
            => new Uri(M_BaseUri, $"?r=explorer/tx&hash={hash}");

        public Uri CreateAddressUrl(string address)
            => new Uri(M_BaseUri, $"?r=explorer/address&hash={address}");

        public Uri CreateBlockUrl(string blockHash)
            => new Uri(M_BaseUri, $"?r=explorer/block&hash={blockHash}");
    }
}