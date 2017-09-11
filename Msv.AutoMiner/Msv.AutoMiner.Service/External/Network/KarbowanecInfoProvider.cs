using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Service.External.Network
{
    public class KarbowanecInfoProvider : WebDownloaderBase, ICoinNetworkInfoProvider
    {
        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic json = JsonConvert.DeserializeObject(
                DownloadString("http://pool.karbowanec.com:8117/live_stats"));
            var difficulty = (double) json.network.difficulty;
            //TODO: get block time
            return new CoinNetworkStatistics
            {
                Difficulty = difficulty,
                NetHashRate = (long) (difficulty / (double) json.config.coinDifficultyTarget),
                BlockReward = (double) json.network.reward / (double) json.config.coinUnits
            };
        }
    }
}
