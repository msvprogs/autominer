﻿using Msv.AutoMiner.Rig.System.Data;

namespace Msv.AutoMiner.Rig.System.Contracts
{
    public interface ISystemStateProvider
    {
        string GetOsName();
        CpuState[] GetCpuStates();
        double GetTotalMemoryMb();
        double GetUsedMemoryMb();
    }
}
