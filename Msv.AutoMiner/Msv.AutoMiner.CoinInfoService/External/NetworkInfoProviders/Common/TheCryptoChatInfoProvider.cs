using System;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common
{
    public class TheCryptoChatInfoProvider : NetworkInfoProviderBase
    {
        private readonly IWebClient m_WebClient;
        private readonly string m_CoinName;

        public TheCryptoChatInfoProvider(IWebClient webClient, string coinName)
        {
            if (string.IsNullOrEmpty(coinName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(coinName));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_CoinName = coinName;
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var miningInfo = (dynamic) JsonConvert.DeserializeObject(
                m_WebClient.DownloadString($"http://{m_CoinName}.thecryptochat.net/api_fetch.php?method=getmininginfo"));
            return new CoinNetworkStatistics
            {
                BlockReward = (double) miningInfo.data.blockvalue / 1e8,
                Difficulty = (double) miningInfo.data.difficulty["proof-of-work"],
                NetHashRate = (long) ((double) miningInfo.data.netmhashps * 1e6),
                Height = (long) miningInfo.data.blocks
            };
        }
    }
}
