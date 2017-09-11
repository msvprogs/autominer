using System.Collections.Generic;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Data;

namespace Msv.AutoMiner.Service.External.Contracts
{
    public interface IMultiCoinNetworkInfoProvider
    {
        Dictionary<string, Dictionary<CoinAlgorithm, CoinNetworkStatistics>> GetMultiNetworkStats();
    }
}