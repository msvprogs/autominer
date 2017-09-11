using System;
using System.Linq;
using System.Text.RegularExpressions;
using Msv.AutoMiner.Service.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.Service.External.Network.Common
{
    public class BchainInfoInfoProvider : NetworkInfoProviderBase
    {
        private static readonly Regex M_DifficultyRegex = new Regex(
            @"var diff = (?<value>(\d+\.)?\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex M_BlockJsonRegex = new Regex(
            @"var blocks = (?<json>{.*?})", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex M_TxJsonRegex = new Regex(
            @"var ctx = (?<json>\[.*\]);", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private readonly string m_CurrencySymbol;

        public BchainInfoInfoProvider(string currencySymbol)
        {
            if (string.IsNullOrEmpty(currencySymbol))
                throw new ArgumentException("Value cannot be null or empty.", nameof(currencySymbol));
            m_CurrencySymbol = currencySymbol;
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var result = DownloadString($"https://bchain.info/{m_CurrencySymbol}/");
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
                .Select(x => new BlockInfo
                {
                    Height = long.Parse(x.Name),
                    Timestamp = ((JArray) x.Value)[0].Value<long>()
                })
                .ToArray();
            var height = blocks.Max(x => x.Height);
            var blockResult = DownloadString($"https://bchain.info/{m_CurrencySymbol}/block/{height}");
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
