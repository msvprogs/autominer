using System.Collections.Generic;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.NetworkInfo.Data;

namespace Msv.AutoMiner.NetworkInfo
{
    public interface IMultiNetworkInfoProvider : IBlockExplorerUrlProvider
    {
        Dictionary<string, Dictionary<KnownCoinAlgorithm, CoinNetworkStatistics>> GetMultiNetworkStats();
    }
}