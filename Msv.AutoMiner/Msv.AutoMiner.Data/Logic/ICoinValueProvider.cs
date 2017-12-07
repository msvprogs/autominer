using System;

namespace Msv.AutoMiner.Data.Logic
{
    public interface ICoinValueProvider
    {
        CoinValue[] GetCurrentCoinValues(bool activeOnly);
        CoinValue[] GetAggregatedCoinValues(bool activeOnly, DateTime minDateTime);
    }
}
