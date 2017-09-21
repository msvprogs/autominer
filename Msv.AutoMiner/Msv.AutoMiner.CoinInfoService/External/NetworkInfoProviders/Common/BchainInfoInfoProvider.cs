using System;
using System.Linq;
using System.Text.RegularExpressions;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common
{
    public class BchainInfoInfoProvider : NetworkInfoProviderBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://bchain.info");

        private static readonly Regex M_DifficultyRegex = new Regex(
            @"var diff = (?<value>(\d+\.)?\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex M_BlockJsonRegex = new Regex(
            @"var blocks = (?<json>{.*?})", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex M_TxJsonRegex = new Regex(
            @"var ctx = (?<json>\[.*\]);", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

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
            var result = m_WebClient.DownloadString(new Uri(M_BaseUri, $"/{m_CurrencySymbol}/"));
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
            var blockResult = m_WebClient.DownloadString(new Uri(M_BaseUri, $"/{m_CurrencySymbol}/block/{height}"));
            var txsMatch = M_TxJsonRegex.Match(blockResult);
            double? reward = null;
            if (txsMatch.Success)
            {
                var txsJson = JsonConvert.DeserializeObject<JArray>(txsMatch.Groups["json"].Value);
                reward = txsJson.Cast<dynamic>()
                    .Where(x => x.@in.Count > 0 && x.@in[0].coinbase != null && x.@out != null)
                    .Select(x => ((JArray) x.@out).Cast<dynamic>().Sum(y => (double?) y.value))
                    .FirstOrDefault();
            }
            return new CoinNetworkStatistics
            {
                Difficulty = difficulty,
                BlockTimeSeconds = CalculateBlockStats(blocks)?.MeanBlockTime,
                Height = height,
                BlockReward = reward / 1e8
            };
        }
    }
}
