using System;
using System.Linq;
using Msv.AutoMiner.Service.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.Service.External.Network.Common
{
    public class InsightInfoProvider : NetworkInfoProviderBase
    {
        private readonly Uri m_BaseUrl;

        public InsightInfoProvider(string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(baseUrl));

            m_BaseUrl = new Uri(baseUrl);
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            dynamic blocksJson = JsonConvert.DeserializeObject<JObject>(
                DownloadString(m_BaseUrl + "/blocks"));
            var blockInfos = ((JArray)blocksJson.blocks)
                .Cast<dynamic>()
                .Where(IsUsableBlock)
                .Select(x => new
                {
                    Height = (long)x.height,
                    BlockHash = (string)x.hash,
                    Timestamp = (long)x.time
                })
                .ToArray();
            dynamic infoJson = JsonConvert.DeserializeObject(
                DownloadString(m_BaseUrl + "/status?q=getInfo"));
            var stats = CalculateBlockStats(blockInfos
                .Select(x => new BlockInfo
                {
                    Height = x.Height,
                    Timestamp = x.Timestamp
                }));
            var lastBlockTransactions = JsonConvert.DeserializeObject<JObject>(
                DownloadString(m_BaseUrl + $"/txs?block={blockInfos[0].BlockHash}&pageNum=0"));
            var reward = ((JArray)lastBlockTransactions["txs"])
                .Where(x => ((JArray)x["vin"]).Cast<dynamic>().Any(y => y.coinbase != null))
                .Select(x => ((JArray)x["vout"]).Select(y => y["value"].Value<double?>()).Sum())
                .FirstOrDefault();
            return new CoinNetworkStatistics
            {
                Difficulty = GetDifficulty(infoJson.info),
                Height = (long)infoJson.info.blocks,
                BlockReward = reward,
                BlockTimeSeconds = stats?.MeanBlockTime
            };
        }

        protected virtual bool IsUsableBlock(dynamic blockInfo) => true;
        protected virtual double GetDifficulty(dynamic statsInfo) => (double) statsInfo.difficulty;
    }
}
