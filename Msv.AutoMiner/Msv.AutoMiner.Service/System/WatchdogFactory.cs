using Msv.AutoMiner.Commons;
using Msv.AutoMiner.Service.System.Contracts;
using Msv.AutoMiner.Service.System.Unix;
using Msv.AutoMiner.Service.System.Windows;

namespace Msv.AutoMiner.Service.System
{
    public class WatchdogFactory : PlatformSpecificFactoryBase<IWatchdog>
    {
        protected override IWatchdog CreateForUnix() => new SoftwareWatchdog();

        protected override IWatchdog CreateForWindows() => new MockWatchdog();
    }
}
