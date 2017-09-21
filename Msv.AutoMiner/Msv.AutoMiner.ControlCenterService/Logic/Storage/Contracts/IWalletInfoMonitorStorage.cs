using System;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts
{
    public interface IWalletInfoMonitorStorage
    {
        Wallet[] GetActiveWallets();

        void StoreWalletBalances(WalletBalance[] balances);
        WalletOperation[] LoadExistingOperations(string[] externalIds, DateTime startDate);
        void StoreWalletOperations(WalletOperation[] operations);
    }
}
