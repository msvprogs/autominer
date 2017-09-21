using System;
using Msv.AutoMiner.ControlCenterService.External.Data;

namespace Msv.AutoMiner.ControlCenterService.External.Contracts
{
    public interface IPoolInfoProvider
    {
        PoolInfo GetInfo(DateTime minPaymentDate);
    }
}