using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common
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
            dynamic blocksJson = JsonConvert.DeserializeObject<JObject>(
                m_WebClient.DownloadString(m_BaseUrl + "/blocks"));
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
                m_WebClient.DownloadString(m_BaseUrl + "/status?q=getInfo"));
            var stats = CalculateBlockStats(blockInfos
                .Select(x => new BlockInfo(x.Timestamp, x.Height)));
            var lastBlockTransactions = JsonConvert.DeserializeObject<JObject>(
                m_WebClient.DownloadString(m_BaseUrl + $"/txs?block={blockInfos[0].BlockHash}&pageNum=0"));
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
