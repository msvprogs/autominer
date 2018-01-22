using System;
using System.Linq;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.NetworkInfo.Common
{
    //API: https://chainz.cryptoid.info/api.dws
    public class ChainzCryptoidInfoProvider : NetworkInfoProviderBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://chainz.cryptoid.info");

        private readonly IProxiedWebClient m_WebClient;
        private readonly string m_CurrencySymbol;

        public ChainzCryptoidInfoProvider(IProxiedWebClient webClient, string currencySymbol)
        {
            if (string.IsNullOrEmpty(currencySymbol))
                throw new ArgumentException("Value cannot be null or empty.", nameof(currencySymbol));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_CurrencySymbol = currencySymbol.ToLowerInvariant();
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var hashrate = m_WebClient.DownloadStringProxied(
                new Uri(CreateCurrencyBaseUrl(), "api.dws?q=nethashps").ToString());
            dynamic blocksInfo = JsonConvert.DeserializeObject(m_WebClient.DownloadStringProxied(
                new Uri(M_BaseUri, $"/explorer/index.data.dws?coin={m_CurrencySymbol}&n=20").ToString()));

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

            var bestBlock = blocks.First(x => x.Height == height);
            return new CoinNetworkStatistics
            {
                Difficulty = bestBlock.Difficulty,
                NetHashRate = ParsingHelper.ParseHashRate(hashrate),
                Height = height,
                LastBlockTime = DateTimeHelper.ToDateTimeUtc(bestBlock.Timestamp)
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(CreateCurrencyBaseUrl(), $"tx.dws?{hash}.htm");

        public override Uri CreateAddressUrl(string address)
            => new Uri(CreateCurrencyBaseUrl(), $"address.dws?{address}.htm");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(CreateCurrencyBaseUrl(), $"block.dws?{blockHash}.htm");

        private Uri CreateCurrencyBaseUrl()
            => new Uri(M_BaseUri, $"/{m_CurrencySymbol}/");
    }
}
