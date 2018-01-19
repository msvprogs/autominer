using System;
using System.Collections.Generic;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts
{
    public interface IPoolAvailabilityMonitorStorage
    {
        Pool[] GetActivePools();
        void SavePoolResponseStoppedDates(Dictionary<int, DateTime?> dates);
        Wallet GetBitCoinMiningTarget();
    }
}
