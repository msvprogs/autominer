using System;
using System.Diagnostics;

namespace Msv.AutoMiner.Service.System.Contracts
{
    public interface IChildProcessTracker : IDisposable
    {
        void StartTracking(Process process);
    }
}
