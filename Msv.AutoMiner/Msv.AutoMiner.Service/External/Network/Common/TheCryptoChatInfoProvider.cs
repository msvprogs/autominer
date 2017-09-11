using System;
using Msv.AutoMiner.Service.Data;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Service.External.Network.Common
{
    public class TheCryptoChatInfoProvider : NetworkInfoProviderBase
    {
        private readonly string m_CoinName;

        public TheCryptoChatInfoProvider(string coinName)
        {
            if (string.IsNullOrEmpty(coinName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(coinName));

            m_CoinName = coinName;
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var miningInfo = (dynamic) JsonConvert.DeserializeObject(
                DownloadString($"http://{m_CoinName}.thecryptochat.net/api_fetch.php?method=getmininginfo"));
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
