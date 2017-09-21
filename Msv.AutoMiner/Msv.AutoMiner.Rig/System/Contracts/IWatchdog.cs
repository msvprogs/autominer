using System;

namespace Msv.AutoMiner.Rig.System.Contracts
{
    public interface IWatchdog : IDisposable
    {
        void Feed();
    }
}
