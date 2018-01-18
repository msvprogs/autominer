using System;

namespace Msv.AutoMiner.Data.Logic
{
    public interface ICoinNetworkInfoProvider
    {
        CoinNetworkInfo[] GetCurrentNetworkInfos(bool activeOnly, DateTime? dateTime = null);
        CoinNetworkInfo[] GetAggregatedNetworkInfos(bool activeOnly, DateTime minDateTime);
    }
}
