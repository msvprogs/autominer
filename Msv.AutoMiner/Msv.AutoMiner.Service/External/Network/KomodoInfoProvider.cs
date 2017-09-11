using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Network.Common;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Service.External.Network
{
    public class KomodoInfoProvider : InsightInfoProvider
    {
        public KomodoInfoProvider() 
            : base("http://kmd.explorer.supernet.org/api")
        { }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            var stats = base.GetNetworkStats();

            dynamic hashRateJson = JsonConvert.DeserializeObject(
                DownloadString("http://www.komodopool.com/api/stats"));
            stats.NetHashRate = (long) ((double) hashRateJson.algos.equihash.hashrate / 1e4);
            return stats;
        }
    }
}
