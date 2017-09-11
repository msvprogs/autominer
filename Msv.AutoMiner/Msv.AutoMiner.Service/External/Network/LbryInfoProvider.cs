using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Network.Common;
using Msv.AutoMiner.Service.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable ArgumentsStyleLiteral

namespace Msv.AutoMiner.Service.External.Network
{
    public class LbryInfoProvider : NetworkInfoProviderBase
    {
        private static readonly Dictionary<string, string> M_Headers =
            new Dictionary<string, string>
            {
                ["Referer"] = "https://explorer.lbry.io/",
                ["Authorization"] = "Basic ZWxhc3RpYzpjaGFuZ2VtZQ==",
                ["Origin"] = "https://explorer.lbry.io",
                ["Content-Type"] = "application/json",
                ["User-Agent"] = UserAgent
            };

        public override CoinNetworkStatistics GetNetworkStats()
        {
            // Lbry's block explorer certificate has been expired and isn't replaced yet
            dynamic blocks = JsonConvert.DeserializeObject(
                UploadString(
                    "https://elastic.cortesexplorer.com:9200/lbry-blocks/_search",
                    "{\"query\":{\"match_all\":{}},\"size\":10,\"sort\":{\"@timestamp\":\"desc\"}}",
                    M_Headers,
                    skipCertificateValidation: true));

            var blockInfos = ((JArray) blocks.hits.hits)
                .Cast<dynamic>()
                .Select(x => new
                {
                    DateTime = (DateTime) x._source["@timestamp"],
                    Difficulty = (double) x._source.difficulty,
                    Height = (long) x._source.height
                })
                .OrderByDescending(x => x.DateTime)
                .ToArray();

            var height = blockInfos.Max(x => x.Height);
            dynamic transactions = JsonConvert.DeserializeObject(
                UploadString(
                    "https://elastic.cortesexplorer.com:9200/lbry-transactions/_search",
                    "{\"query\":{\"constant_score\":{\"filter\":{\"term\":{\"block_height\":\""
                    + height + "\"}}}},\"size\":4}", 
                    M_Headers,
                    skipCertificateValidation: true));

            var reward = ((JArray) transactions.hits.hits)
                .Cast<dynamic>()
                .Where(x => x._source.vin.Count > 0 && x._source.vin[0].coinbase != null)
                .Select(x => ((JArray) x._source.vout).Cast<dynamic>().Sum(y => (double?) y.value))
                .FirstOrDefault();

            return new CoinNetworkStatistics
            {
                Difficulty = blockInfos.First().Difficulty,
                BlockReward = reward,
                Height = height,
                BlockTimeSeconds = CalculateBlockStats(blockInfos
                    .Select(x => new BlockInfo
                    {
                        Height = x.Height,
                        Timestamp = TimestampHelper.ToTimestamp(x.DateTime, TimeZoneInfo.Utc)
                    }))?.MeanBlockTime
            };
        }
    }
}
