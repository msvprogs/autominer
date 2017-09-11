using System.Diagnostics;
using Msv.AutoMiner.Commons;
using Msv.AutoMiner.Service.System.Contracts;
using Msv.AutoMiner.Service.System.Windows;

namespace Msv.AutoMiner.Service.System
{
    public class ChildProcessTrackerFactory : PlatformSpecificFactoryBase<IChildProcessTracker>
    {
        //Unix kills child process automatically, I appreciate it.
        protected override IChildProcessTracker CreateForUnix() 
            => new DummyProcessTracker();

        protected override IChildProcessTracker CreateForWindows() 
            => new WindowsChildProcessTracker();

        private class DummyProcessTracker : IChildProcessTracker
        {
            public void StartTracking(Process process)
            { }

            public void Dispose()
            { }
        }
    }
}
