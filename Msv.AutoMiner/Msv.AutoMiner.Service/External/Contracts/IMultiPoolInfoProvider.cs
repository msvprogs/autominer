using System;
using Msv.AutoMiner.Service.Data;

namespace Msv.AutoMiner.Service.External.Contracts
{
    public interface IMultiPoolInfoProvider
    {
        PoolInfo[] GetInfos(DateTime minPaymentDate);
    }
}