using System;
using System.Collections.Generic;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.External.Contracts
{
    public interface IMultiPoolInfoProvider
    {
        IReadOnlyDictionary<Pool, PoolInfo> GetInfo(DateTime minPaymentDate);
    }
}