using System;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using Msv.AutoMiner.Service.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.Service.External.Network.Common
{
    //API: https://chainz.cryptoid.info/api.dws
    public class ChainzCryptoidInfoProvider : NetworkInfoProviderBase
    {
        private readonly string m_CurrencySymbol;

        private static readonly object M_SyncRoot = new object();

        public ChainzCryptoidInfoProvider(string currencySymbol)
        {
            if (string.IsNullOrEmpty(currencySymbol))
                throw new ArgumentException("Value cannot be null or empty.", nameof(currencySymbol));

            m_CurrencySymbol = currencySymbol.ToLowerInvariant();
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            //This site doesn't like multiple requests from same IP.
            lock (M_SyncRoot)
            {
                var hashrate = DownloadString($"http://chainz.cryptoid.info/{m_CurrencySymbol}/api.dws?q=nethashps");
                dynamic blocksInfo = JsonConvert.DeserializeObject<JObject>(DownloadString(
                    $"https://chainz.cryptoid.info/explorer/index.data.dws?coin={m_CurrencySymbol}&n=20"));

                var blocks = ((JArray)blocksInfo.blocks)
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
                var hashPage = new HtmlDocument();
                hashPage.LoadHtml(DownloadString($"https://chainz.cryptoid.info/{m_CurrencySymbol}/block.dws?{height}.htm"));
                var hash = hashPage.DocumentNode.SelectSingleNode("//code[@class='hash']").InnerText;

                var reward = JsonConvert.DeserializeObject<JArray>(DownloadString(
                        $"https://chainz.cryptoid.info/explorer/block.txs.dws?coin={m_CurrencySymbol}&h={hash}&fmt.js"))
                    .Cast<dynamic>()
                    .Where(x => ((JToken)x.inputs).Values<string>().Any(y => y.Contains("Generation")))
                    .Select(x => (double)x.v)
                    .First();

                return new CoinNetworkStatistics
                {
                    Difficulty = blocks.First().Difficulty,
                    NetHashRate = (long)double.Parse(hashrate, CultureInfo.InvariantCulture),
                    Height = height,
                    BlockReward = reward,
                    BlockTimeSeconds = CalculateBlockStats(blocks
                        .Select(x => new BlockInfo
                        {
                            Height = x.Height,
                            Timestamp = x.Timestamp
                        }))?.MeanBlockTime
                };
            }
        }
    }
}
