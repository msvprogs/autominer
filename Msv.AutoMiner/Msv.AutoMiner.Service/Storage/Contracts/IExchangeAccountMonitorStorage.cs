using System;
using System.Collections.Generic;
using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.Storage.Contracts
{
    public interface IExchangeAccountMonitorStorage
    {
        int? GetBitCoinId();
        Coin[] GetCoinsWithExchanges();
        Dictionary<ExchangeType, DateTime> GetLastOperationDates();
        void SaveBalances(ExchangeAccountBalance[] balances);
        void SaveOperations(ExchangeAccountOperation[] operations);
    }
}
