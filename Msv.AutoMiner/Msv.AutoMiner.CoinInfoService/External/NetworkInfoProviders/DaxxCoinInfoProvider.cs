using System;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    public class DaxxCoinInfoProvider : INetworkInfoProvider
    {
        private readonly IWebClient m_WebClient;

        public DaxxCoinInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic info = JsonConvert.DeserializeObject(m_WebClient.DownloadString(
                "https://daxxcoin.com/api/v1/user/getBlockChainInfo?limit=10"));
            if ((bool)info.error)
                throw new ExternalDataUnavailableException((string)info.message);
            var block = info.response.blocks[0];
            return new CoinNetworkStatistics
            {
                Height = (long) block.blockNumber,
                Difficulty = (double) block.difficulty,
            };
        }
    }
}
