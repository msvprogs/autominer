using System;
using Msv.AutoMiner.ControlCenterService.External.Data;

namespace Msv.AutoMiner.ControlCenterService.External.Contracts
{
    public interface IWalletInfoProvider
    {
        WalletBalanceData[] GetBalances();
        WalletOperationData[] GetOperations(DateTime startDate);
    }
}