using System.Collections.Generic;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.CoinInfoService.External.Contracts
{
    public interface IMultiNetworkInfoProvider
    {
        Dictionary<string, Dictionary<KnownCoinAlgorithm, CoinNetworkStatistics>> GetMultiNetworkStats();
    }
}