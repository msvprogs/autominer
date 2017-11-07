using System;
using System.Collections.Generic;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts
{
    public interface IPoolInfoMonitorStorage
    {
        Pool[] GetActivePools();
        void StorePoolAccountStates(PoolAccountState[] poolAccountStates);
        void StorePoolPayments(PoolPayment[] poolPayments);
        PoolPayment[] LoadExistingPayments(string[] externalIds, DateTime startDate);
        Dictionary<string, int> GetWalletIds(string[] addresses);
    }
}
