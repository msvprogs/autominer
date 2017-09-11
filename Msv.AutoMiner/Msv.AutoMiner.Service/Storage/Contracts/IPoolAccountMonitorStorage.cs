using System;
using System.Collections.Generic;
using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.Storage.Contracts
{
    public interface IPoolAccountMonitorStorage
    {
        Pool[] GetPools();
        Dictionary<int, DateTime> GetPoolLastPaymentDates();
        void SaveAccountStates(PoolAccountState[] states);
        void SavePoolPayments(PoolPayment[] payments);
    }
}
