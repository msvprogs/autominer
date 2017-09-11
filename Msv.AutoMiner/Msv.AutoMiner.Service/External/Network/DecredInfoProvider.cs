using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Service.External.Network
{
    public class DecredInfoProvider : WebDownloaderBase, ICoinNetworkInfoProvider
    {
        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic json = JsonConvert.DeserializeObject(DownloadString("https://dcrstats.com/api/v1/get_stats"));
            return new CoinNetworkStatistics
            {
                Difficulty = (double) json.difficulty,
                BlockReward = (double) json.pow_reward,
                BlockTimeSeconds = (double) json.average_time,
                NetHashRate = (long) json.networkhashps,
                Height = (long) json.blocks
            };
        }
    }
}
