using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.NetworkInfo.Common
{
    public class IquidusMultiAlgoNetworkInfoProvider : IMultiNetworkInfoProvider
    {
        private readonly IWebClient m_WebClient;
        private readonly Uri m_BaseUrl;
        private readonly string m_CurrencySymbol;
        private readonly Dictionary<int, KnownCoinAlgorithm?> m_AlgoMappings;

        public IquidusMultiAlgoNetworkInfoProvider(
            IWebClient webClient, Uri baseUri, string currencySymbol, Dictionary<int, KnownCoinAlgorithm?> algoMappings)
        {
            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_BaseUrl = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
            m_CurrencySymbol = currencySymbol ?? throw new ArgumentNullException(nameof(currencySymbol));
            m_AlgoMappings = algoMappings ?? throw new ArgumentNullException(nameof(algoMappings));
        }

        public Dictionary<string, Dictionary<KnownCoinAlgorithm, CoinNetworkStatistics>> GetMultiNetworkStats()
        {
            dynamic txs = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(m_BaseUrl, "/ext/getlasttxs/1/100")));
            var height = (long)txs.data[0].blockindex;

            var difficulties = ((JArray)txs.data)
                .Cast<dynamic>()
                .Where(x => (string)x.vin[0].addresses == "coinbase")
                .Where(x => (double)x.vin[0].amount > 0)
                .Select(x => (string)x.blockhash)
                .Distinct()
                .Select(x => m_WebClient.DownloadString(new Uri(m_BaseUrl, "/api/getblock?hash=" + x)))
                .Select(JsonConvert.DeserializeObject)
                .Cast<dynamic>()
                .Where(x => ((string)x.flags).Contains("proof-of-work"))
                .Select(x => (
                    algo: m_AlgoMappings.TryGetValue((int)x.algo_id),
                    difficulty: (double)x.difficulty,
                    time: (long)x.time))
                .Where(x => x.algo != null)
                .Distinct(new AlgoTupleEqualityComparer())
                .Take(m_AlgoMappings.Count)
                .ToDictionary(x => x.algo.Value, x => new CoinNetworkStatistics
                {
                    Height = height,
                    Difficulty = x.difficulty,
                    LastBlockTime = DateTimeHelper.ToDateTimeUtc(x.time)
                });

            var maxBlockTime = difficulties.Max(x => x.Value.LastBlockTime);
            difficulties.ForEach(x => x.Value.LastBlockTime = maxBlockTime);
            return new Dictionary<string, Dictionary<KnownCoinAlgorithm, CoinNetworkStatistics>>
            {
                [m_CurrencySymbol] = difficulties
            };
        }

        public Uri CreateTransactionUrl(string hash)
            => new Uri(m_BaseUrl, $"tx/{hash}");

        public Uri CreateAddressUrl(string address)
            => new Uri(m_BaseUrl, $"address/{address}");

        public Uri CreateBlockUrl(string blockHash)
            => new Uri(m_BaseUrl, $"block/{blockHash}");

        private class AlgoTupleEqualityComparer : EqualityComparer<(KnownCoinAlgorithm? algo, double difficulty, long time)>
        {
            public override bool Equals(
                (KnownCoinAlgorithm? algo, double difficulty, long time) x,
                (KnownCoinAlgorithm? algo, double difficulty, long time) y)
                => x.algo == y.algo;

            public override int GetHashCode((KnownCoinAlgorithm? algo, double difficulty, long time) obj)
                => obj.algo.GetHashCode();
        }
    }
}
