using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Msv.AutoMiner.Service.External.Network.Common;

namespace Msv.AutoMiner.Service.External.Network
{
    public class ByteCoinInfoProvider : ICoinNetworkInfoProvider
    {
        public CoinNetworkStatistics GetNetworkStats()
        {
            var poolStats = new MinerGateInfoProvider("BCN").GetNetworkStats();
            var explorerStats = new ChainRadarInfoProvider("BCN").GetNetworkStats();
            poolStats.BlockTimeSeconds = explorerStats.BlockTimeSeconds;
            return poolStats;
        }
    }
}
