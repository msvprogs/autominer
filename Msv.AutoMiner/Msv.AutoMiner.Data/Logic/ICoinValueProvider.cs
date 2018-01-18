using System;

namespace Msv.AutoMiner.Data.Logic
{
    public interface ICoinValueProvider
    {
        CoinValue[] GetCurrentCoinValues(bool activeOnly, DateTime? dateTime = null);
        CoinValue[] GetAggregatedCoinValues(bool activeOnly, DateTime minDateTime);
    }
}
