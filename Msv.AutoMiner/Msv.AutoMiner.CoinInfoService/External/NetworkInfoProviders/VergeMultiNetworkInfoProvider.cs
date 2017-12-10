using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    public class VergeMultiNetworkInfoProvider : IMultiNetworkInfoProvider
    {
        private static readonly Uri M_BaseUri = new Uri("https://verge-blockchain.info");

        private static readonly Dictionary<int, KnownCoinAlgorithm?> M_AlgoIds =
            new Dictionary<int, KnownCoinAlgorithm?>
            {
                [1] = KnownCoinAlgorithm.X17,
                [2] = KnownCoinAlgorithm.Lyra2Rev2,
                [3] = KnownCoinAlgorithm.Blake2S,
                [4] = KnownCoinAlgorithm.MyriadGroestl
            };

        private readonly IWebClient m_WebClient;

        public VergeMultiNetworkInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public Dictionary<string, Dictionary<KnownCoinAlgorithm, CoinNetworkStatistics>> GetMultiNetworkStats()
        {
            var height = long.Parse(m_WebClient.DownloadString(new Uri(M_BaseUri, "/api/getblockcount")));
            dynamic txs = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_BaseUri, "/ext/getlasttxs/1/100")));
            var difficulties = ((JArray) txs.data)
                .Cast<dynamic>()
                .Where(x => (string) x.vin[0].addresses == "coinbase")
                .Select(x => (string) x.blockhash)
                .Distinct()
                .Select(x => m_WebClient.DownloadString(new Uri(M_BaseUri, "/api/getblock?hash=" + x)))
                .Select(JsonConvert.DeserializeObject)
                .Cast<dynamic>()
                .Select(x => (algo: M_AlgoIds.TryGetValue((int) x.algo_id), difficulty: (double) x.difficulty))
                .Where(x => x.algo != null)
                .Distinct(new AlgoTupleEqualityComparer())
                .Take(M_AlgoIds.Count)
                .ToDictionary(x => x.algo.Value, x => new CoinNetworkStatistics
                {
                    Height = height,
                    Difficulty = x.difficulty
                });
            return new Dictionary<string, Dictionary<KnownCoinAlgorithm, CoinNetworkStatistics>>
            {
                ["XVG"] = difficulties
            };
        }

        private class AlgoTupleEqualityComparer : EqualityComparer<(KnownCoinAlgorithm? algo, double difficulty)>
        {
            public override bool Equals(
                (KnownCoinAlgorithm? algo, double difficulty) x,
                (KnownCoinAlgorithm? algo, double difficulty) y)
                => x.algo == y.algo;

            public override int GetHashCode((KnownCoinAlgorithm? algo, double difficulty) obj)
                => obj.algo.GetHashCode();
        }
    }
}
