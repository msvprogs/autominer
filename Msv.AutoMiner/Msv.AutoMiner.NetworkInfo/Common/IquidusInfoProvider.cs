using System;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;
using Msv.AutoMiner.NetworkInfo.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

            var lastPoWBlock = m_Options.GetDifficultyFromLastPoWBlock
                ? new BlockChainSearcher(x => JsonConvert.DeserializeObject<BlockHeader>(
                        m_WebClient.DownloadString(new Uri(m_BaseUrl, "/api/getblock?hash=" + x))))
                    .SearchPoWBlock(lastBlockInfo)
                : lastBlockInfo;

            dynamic lastTransactionsJson = JsonConvert.DeserializeObject(m_WebClient.DownloadString(
                new Uri(m_BaseUrl, "/ext/getlasttxs/0.0000001")));
            var lastTransactionsData = ((JArray) lastTransactionsJson.data)
                .Cast<dynamic>()
                .Where(x => (string) x.blockhash == lastPoWBlock.Hash)
                .Where(x => x.vin != null &&
                            x.vout != null) //yeah, some versions of Iquidus don't return ins and outs of tx
                .Select(x => new
                {
                    TransactionInfo = new TransactionInfo
                    {
                        InValues = ((JArray) x.vin)
                            .Cast<dynamic>()
                            .Where(y => (string) y.addresses != "coinbase")
                            .Select(y => (double) y.amount / 1e8)
                            .ToArray(),
                        OutValues = ((JArray) x.vout)
                            .Cast<dynamic>()
                            .Select(y => (double) y.amount / 1e8)
                            .ToArray()
                    },
                    Hash = (string) x.txid
                })
                .ToArray();

            // Just in case the previous request didn't return some of our transactions (this happens in bugged Iquidus explorers)
            var missedTransactions = lastPoWBlock.Transactions
                .Except(lastTransactionsData.Select(x => x.Hash), StringComparer.InvariantCultureIgnoreCase)
                .Select(ParseTransactionFromHtml)
                .ToArray();

            var masternodeCount = stats.data[0].masternodeCountOnline;
            return new CoinNetworkStatistics
            {
                Difficulty = lastPoWBlock.Difficulty,
                NetHashRate = double.TryParse(
                    (string) stats.data[0].hashrate, NumberStyles.Any, CultureInfo.InvariantCulture, out var hashRate)
                    ? GetRealHashRate(hashRate)
                    : 0,
                Height = height,
                LastBlockTime = DateTimeHelper.ToDateTimeUtc(lastBlockInfo.Time),
                LastBlockTransactions = lastTransactionsData
                    .Select(x => x.TransactionInfo)
                    .Concat(missedTransactions)
                    .ToArray(),
                MasternodeCount = masternodeCount != null
                    ? masternodeCount is JObject
                        ? (int) masternodeCount.total
                        : (int) masternodeCount
                    : (int?) null
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(m_BaseUrl, $"tx/{hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(m_BaseUrl, $"address/{address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(m_BaseUrl, $"block/{blockHash}");

        protected virtual double GetDifficulty(dynamic difficultyValue)
            => (double)difficultyValue;

        private TransactionInfo ParseTransactionFromHtml(string hash)
        {
            var html = new HtmlDocument();
            html.LoadHtml(m_WebClient.DownloadString(CreateTransactionUrl(hash)));

            var ins = (html.DocumentNode
                .SelectSingleNode("//div[@class='panel-heading' and contains(., 'Input Addresses')]")
                ?.SelectNodes(".//following-sibling::table/tbody[not(contains(., 'New Coins'))]/tr[not(@class)]/td[2]"))
                .EmptyIfNull()
                .Select(x => ParsingHelper.ParseDouble(x.InnerText))
                .ToArray();

            var outs = (html.DocumentNode
                .SelectSingleNode("//div[@class='panel-heading' and contains(., 'Recipients')]")
                ?.SelectNodes(".//following-sibling::table/tbody/tr[not(@class)]/td[2]"))
                .EmptyIfNull()
                .Select(x => ParsingHelper.ParseDouble(x.InnerText))
                .ToArray();

            return new TransactionInfo
            {
                InValues = ins,
                OutValues = outs
            };
        }

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
