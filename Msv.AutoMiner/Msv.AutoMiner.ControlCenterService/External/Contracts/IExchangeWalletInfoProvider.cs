using System;
using Msv.AutoMiner.ControlCenterService.External.Data;

namespace Msv.AutoMiner.ControlCenterService.External.Contracts
{
    public interface IExchangeWalletInfoProvider
    {
        WalletBalanceData[] GetBalances();
        WalletOperationData[] GetOperations(DateTime startDate);
    }
}