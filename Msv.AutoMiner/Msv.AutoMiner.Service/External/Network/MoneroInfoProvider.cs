using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Network.Common;

namespace Msv.AutoMiner.Service.External.Network
{
    //API: https://moneroblocks.info/api
    public class MoneroInfoProvider : NetworkInfoProviderBase
    {
        public override CoinNetworkStatistics GetNetworkStats()
        {
            var poolStats = new MinerGateInfoProvider("XMR").GetNetworkStats();
            var explorerStats = new ChainRadarInfoProvider("XMR").GetNetworkStats();
            poolStats.BlockTimeSeconds = explorerStats.BlockTimeSeconds;
            return poolStats;
        }
    }
}
