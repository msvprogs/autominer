using Msv.AutoMiner.Rig.System.Contracts;
using NLog;

namespace Msv.AutoMiner.Rig.System.Windows
{
    public class MockWatchdog : IWatchdog
    {
        public MockWatchdog()
        {
            LogManager.GetCurrentClassLogger().Debug("Software Watchdog for Windows isn't implemented");
        }

        public void Dispose()
        { }

        public void Feed()
        { }
    }
}
