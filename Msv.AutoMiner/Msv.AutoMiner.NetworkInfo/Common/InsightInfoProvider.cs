using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.NetworkInfo.Common
{
    public class InsightInfoProvider : NetworkInfoProviderBase
    {
        private readonly IWebClient m_WebClient;
        private readonly Uri m_BaseUrl;

        public InsightInfoProvider(IWebClient webClient, string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(baseUrl));
            
            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_BaseUrl = new Uri(baseUrl);
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var infoJson = m_WebClient.DownloadJsonAsDynamic(m_BaseUrl + "/status?q=getInfo");
            var blockJson = m_WebClient.DownloadJsonAsDynamic(m_BaseUrl + "/blocks?limit=1");

            string bestBlockHash = blockJson.blocks[0].hash;
            var transactions = new List<TransactionInfo>();
            var currentPage = 0;
            int totalPages;
            do
            {
                var bestBlockTransactions = m_WebClient.DownloadJsonAsDynamic(
                    m_BaseUrl + $"/txs?block={bestBlockHash}&pageNum={currentPage++}");
                totalPages = (int) bestBlockTransactions.pagesTotal;
                transactions.AddRange(((JArray)bestBlockTransactions.txs)
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
                            .ToArray(),
                        Fee = (double?)x.fees
                    }));
            } while (totalPages > currentPage);

            return new CoinNetworkStatistics
            {
                Difficulty = GetDifficulty(infoJson.info),
                Height = (long)infoJson.info.blocks,
                LastBlockTime = DateTimeHelper.ToDateTimeUtc((long)blockJson.blocks[0].time),
                LastBlockTransactions = transactions.ToArray()
            };
        }

        public override WalletBalance GetWalletBalance(string address)
        {
            var balanceJson = m_WebClient.DownloadJsonAsDynamic(m_BaseUrl + $"/addr/{address}/?noTxList=1");
            return new WalletBalance
            {
                Available = (double) balanceJson.balance,
                Unconfirmed = (double) balanceJson.unconfirmedBalance
            };
        }

        public override BlockExplorerWalletOperation[] GetWalletOperations(string address, DateTime startDate)
        {
            JArray transactions = m_WebClient.DownloadJsonAsDynamic(m_BaseUrl + $"/txs?address={address}&pageNum=0").txs;
            return transactions
                .Cast<dynamic>()
                .Select(x => new BlockExplorerWalletOperation
                {
                    Address = address,
                    Amount = ((JArray) x.vin)
                             .Cast<dynamic>()
                             .Where(y => (string) y.addr == address)
                             .Select(y => -(double?) y.value)
                             .FirstOrDefault()
                                 ?? ((JArray) x.vout)
                                 .Cast<dynamic>()
                                 .Where(y => y.scriptPubKey?.addresses != null
                                             && ((JArray) y.scriptPubKey.addresses).Any(z => z.Value<string>() == address))
                                 .Select(y => (double) y.value)
                                 .FirstOrDefault(),
                    DateTime = DateTimeHelper.ToDateTimeUtc((long) x.time),
                    Transaction = (string) x.txid
                })
                .Where(x => x.DateTime > startDate)
                .ToArray();
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(m_BaseUrl, $"/tx/{hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(m_BaseUrl, $"/address/{address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(m_BaseUrl, $"/block/{blockHash}");

        protected virtual double GetDifficulty(dynamic statsInfo) 
            => (double) statsInfo.difficulty;
    }
}
