using System;
using Msv.AutoMiner.Service.Data;

namespace Msv.AutoMiner.Service.External.Contracts
{
    public interface IExchangeTrader
    {
        ExchangeAccountBalanceData[] GetBalances();
        ExchangeAccountOperationData[] GetOperations(DateTime startDate);
    }
}