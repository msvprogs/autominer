using System;

namespace Msv.AutoMiner.Data.Logic
{
    public interface ICoinNetworkInfoProvider
    {
        CoinNetworkInfo[] GetCurrentNetworkInfos(bool activeOnly);
        CoinNetworkInfo[] GetAggregatedNetworkInfos(bool activeOnly, DateTime minDateTime);
    }
}
