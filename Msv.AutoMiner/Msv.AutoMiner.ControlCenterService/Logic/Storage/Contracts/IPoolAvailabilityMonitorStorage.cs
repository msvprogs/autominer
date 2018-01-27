using System;
using System.Collections.Generic;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts
{
    public interface IPoolAvailabilityMonitorStorage
    {
        Pool[] GetActivePools();
        void SavePoolAvailabilities(Dictionary<Pool, (PoolAvailabilityState availability, DateTime? date)> availabilities);
        Wallet GetBitCoinMiningTarget();
    }
}
