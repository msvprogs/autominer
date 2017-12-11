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

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common
{
    public class IquidusMultiAlgoNetworkInfoProvider : IMultiNetworkInfoProvider
    {
        private readonly IWebClient m_WebClient;
        private readonly Uri m_BaseUri;
        private readonly string m_CurrencySymbol;
        private readonly Dictionary<int, KnownCoinAlgorithm?> m_AlgoMappings;

        public IquidusMultiAlgoNetworkInfoProvider(
            IWebClient webClient, Uri baseUri, string currencySymbol, Dictionary<int, KnownCoinAlgorithm?> algoMappings)
        {
            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_BaseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
            m_CurrencySymbol = currencySymbol ?? throw new ArgumentNullException(nameof(currencySymbol));
            m_AlgoMappings = algoMappings ?? throw new ArgumentNullException(nameof(algoMappings));
        }

        public Dictionary<string, Dictionary<KnownCoinAlgorithm, CoinNetworkStatistics>> GetMultiNetworkStats()
        {
            dynamic txs = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(m_BaseUri, "/ext/getlasttxs/1/100")));
            var height = (long)txs.data[0].blockindex;

            var difficulties = ((JArray)txs.data)
                .Cast<dynamic>()
                .Where(x => (string)x.vin[0].addresses == "coinbase")
                .Where(x => (double)x.vin[0].amount > 0)
                .Select(x => (string)x.blockhash)
                .Distinct()
                .Select(x => m_WebClient.DownloadString(new Uri(m_BaseUri, "/api/getblock?hash=" + x)))
                .Select(JsonConvert.DeserializeObject)
                .Cast<dynamic>()
                .Where(x => ((string)x.flags).Contains("proof-of-work"))
                .Select(x => (algo: m_AlgoMappings.TryGetValue((int)x.algo_id), difficulty: (double)x.difficulty))
                .Where(x => x.algo != null)
                .Distinct(new AlgoTupleEqualityComparer())
                .Take(m_AlgoMappings.Count)
                .ToDictionary(x => x.algo.Value, x => new CoinNetworkStatistics
                {
                    Height = height,
                    Difficulty = x.difficulty
                });
            return new Dictionary<string, Dictionary<KnownCoinAlgorithm, CoinNetworkStatistics>>
            {
                [m_CurrencySymbol] = difficulties
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
