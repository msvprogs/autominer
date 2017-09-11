using System;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Service.External.Network
{
    public class DaxxCoinInfoProvider : WebDownloaderBase, ICoinNetworkInfoProvider
    {
        public CoinNetworkStatistics GetNetworkStats()
        {
            var info = JsonConvert.DeserializeObject<dynamic>(DownloadString(
                "https://daxxcoin.com/api/v1/user/getBlockChainInfo?limit=10"));
            if ((bool)info.error)
                throw new ApplicationException((string)info.message);
            var block = info.response.blocks[0];
            return new CoinNetworkStatistics
            {
                Height = (long) block.blockNumber,
                Difficulty = (double) block.difficulty,
            };
        }
    }
}
