using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Service.External.Network
{
    //API: https://siamining.com/api/
    public class SiaCoinInfoProvider : WebDownloaderBase, ICoinNetworkInfoProvider
    {
        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic json = JsonConvert.DeserializeObject(DownloadString("https://siamining.com/api/v1/network"));
            return new CoinNetworkStatistics
            {
                Difficulty = (double) json.difficulty,
                NetHashRate = (long) json.hash_rate,
                BlockReward = (double) json.block_reward,
                Height = (long) json.block_height
            };
        }
    }
}
