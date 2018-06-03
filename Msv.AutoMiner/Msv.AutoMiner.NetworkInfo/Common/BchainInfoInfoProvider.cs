using System;
using System.Linq;
using System.Text.RegularExpressions;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.NetworkInfo.Common
{
    public class BchainInfoInfoProvider : NetworkInfoProviderBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://bchain.info");

        private static readonly Regex M_DifficultyRegex = new Regex(
            @"var diff = (?<value>(\d+\.)?\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex M_BlockJsonRegex = new Regex(
            @"var blocks = (?<json>{.*?})", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex M_TransactionsJsonRegex = new Regex(
            @"var ctx = (?<json>\[.*?\]);\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private readonly IWebClient m_WebClient;
        private readonly string m_CurrencySymbol;

        public BchainInfoInfoProvider(IWebClient webClient, string currencySymbol)
        {
            if (string.IsNullOrEmpty(currencySymbol))
                throw new ArgumentException("Value cannot be null or empty.", nameof(currencySymbol));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_CurrencySymbol = currencySymbol.ToUpperInvariant();
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var result = m_WebClient.DownloadString(CreateCurrencyBaseUrl());
            var difficultyMatch = M_DifficultyRegex.Match(result);
            var difficulty = ParsingHelper.ParseDouble(difficultyMatch.Groups["value"].Value);
            var blocksMatch = M_BlockJsonRegex.Match(result);
            if (!blocksMatch.Success)
                return new CoinNetworkStatistics
                {
                    Difficulty = difficulty
                };
            var blocksJson = JsonConvert.DeserializeObject<JObject>(blocksMatch.Groups["json"].Value);
            var blocks = blocksJson.Properties()
                .Select(x => new BlockInfo(((JArray)x.Value)[0].Value<long>(), long.Parse(x.Name)))
                .ToArray();
            var height = blocks.Max(x => x.Height);

            var lastBlockInfoMatch = M_TransactionsJsonRegex.Match(
                m_WebClient.DownloadString(CreateBlockUrl(height.ToString())));
            var lastBlockInfo = JsonConvert.DeserializeObject<JArray>(lastBlockInfoMatch.Groups["json"].Value);

            return new CoinNetworkStatistics
            {
                Difficulty = difficulty,
                BlockTimeSeconds = CalculateBlockStats(blocks)?.MeanBlockTime,
                Height = height,
                LastBlockTime = blocks.OrderByDescending(x => x.Height)
                    .Select(x => (DateTime?)DateTimeHelper.ToDateTimeUtc(x.Timestamp))
                    .DefaultIfEmpty(null)
                    .First(),
                LastBlockTransactions = lastBlockInfo
                    .Cast<dynamic>()
                    .Select(x => new TransactionInfo
                    {
                        InValues = ((JArray)x.@in)
                            .Cast<dynamic>()
                            .Where(y => y.value != null)
                            .Select(y => (double)y.value/1e8)
                            .ToArray(),
                        OutValues = ((JArray)x.@out)
                            .Cast<dynamic>()
                            .Select(y => (double)y.value/1e8)
                            .ToArray()
                    })
                    .ToArray()
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(CreateCurrencyBaseUrl(), "tx/" + hash);

        public override Uri CreateAddressUrl(string address)
            => new Uri(CreateCurrencyBaseUrl(), "addr/" + address);

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(CreateCurrencyBaseUrl(), "block/" + blockHash);

        private Uri CreateCurrencyBaseUrl()
            => new Uri(M_BaseUri, $"/{m_CurrencySymbol}/");
    }
}
