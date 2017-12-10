using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common
{
    //API: https://chainz.cryptoid.info/api.dws
    public class ChainzCryptoidInfoProvider : NetworkInfoProviderBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://chainz.cryptoid.info");

        private readonly IWebClient m_WebClient;
        private readonly string m_CurrencySymbol;

        //Pass DDoSTriggerPreventingWebClient without delays
        public ChainzCryptoidInfoProvider(IWebClient webClient, string currencySymbol)
        {
            if (string.IsNullOrEmpty(currencySymbol))
                throw new ArgumentException("Value cannot be null or empty.", nameof(currencySymbol));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_CurrencySymbol = currencySymbol.ToLowerInvariant();
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var hashrate = m_WebClient.DownloadString(new Uri(M_BaseUri, $"/{m_CurrencySymbol}/api.dws?q=nethashps"));
            dynamic blocksInfo = JsonConvert.DeserializeObject(m_WebClient.DownloadString(
                new Uri(M_BaseUri, $"/explorer/index.data.dws?coin={m_CurrencySymbol}&n=20")));

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

            return new CoinNetworkStatistics
            {
                Difficulty = blocks.First().Difficulty,
                NetHashRate = ParsingHelper.ParseHashRate(hashrate),
                Height = height
            };
        }
    }
}
