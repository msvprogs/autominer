using System;
using Msv.AutoMiner.Data.Logic.Data;

namespace Msv.AutoMiner.Data.Logic.Contracts
{
    public interface ICoinValueProvider
    {
        CoinValue[] GetCurrentCoinValues(bool activeOnly, DateTime? dateTime = null);
        CoinValue[] GetAggregatedCoinValues(bool activeOnly, DateTime minDateTime);
    }
}
