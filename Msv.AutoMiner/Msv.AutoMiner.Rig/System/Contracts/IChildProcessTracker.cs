using System;
using System.Diagnostics;

namespace Msv.AutoMiner.Rig.System.Contracts
{
    public interface IChildProcessTracker : IDisposable
    {
        void StartTracking(Process process);
    }
}
