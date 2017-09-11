using System.Linq;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.Service.External.Network
{
    //API: https://explorer.zcha.in/api
    public class ZcashInfoProvider : WebDownloaderBase, ICoinNetworkInfoProvider
    {
        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic result = JsonConvert.DeserializeObject(DownloadString("https://api.zcha.in/v2/mainnet/network"));
            var transactions = JsonConvert.DeserializeObject<JArray>(DownloadString(
                "https://api.zcha.in/v2/mainnet/transactions?sort=blockHeight&direction=descending&limit=20&offset=0"));
            var rewardTx = transactions
                .Cast<dynamic>()
                .First(x => (bool) x.mainChain && (string) x.type == "minerReward");
            return new CoinNetworkStatistics
            {
                Difficulty = (double) result.difficulty,
                NetHashRate = (long) result.hashrate,
                BlockTimeSeconds = (double) result.meanBlockTime,
                Height = (long) result.blockNumber,
                BlockReward = ((JArray)rewardTx.vout).Cast<dynamic>().Sum(x => (double)x.value)
            };
        }
    }
}
