using System;
using Msv.AutoMiner.ControlCenterService.External.Data;

namespace Msv.AutoMiner.ControlCenterService.External.Contracts
{
    public interface ILocalWalletInfoProvider
    {
        WalletBalanceData GetBalance(string address);
        WalletOperationData[] GetOperations(string address, DateTime startDate);
    }
}
