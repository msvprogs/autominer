using System;
using Msv.AutoMiner.ControlCenterService.External.Data;

namespace Msv.AutoMiner.ControlCenterService.External.Contracts
{
    public interface IMultiPoolInfoProvider
    {
        MultiPoolInfo GetInfo(DateTime minPaymentDate);
    }
}