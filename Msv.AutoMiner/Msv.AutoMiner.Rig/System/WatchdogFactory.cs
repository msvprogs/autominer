using Msv.AutoMiner.Rig.System.Contracts;
using Msv.AutoMiner.Rig.System.Unix;
using Msv.AutoMiner.Rig.System.Windows;

namespace Msv.AutoMiner.Rig.System
{
    public class WatchdogFactory : PlatformSpecificFactoryBase<IWatchdog>
    {
        protected override IWatchdog CreateForUnix() => new UnixSoftwareWatchdog();

        protected override IWatchdog CreateForWindows() => new MockWatchdog();
    }
}
