using Msv.AutoMiner.Rig.System.Contracts;
using Msv.AutoMiner.Rig.System.Unix;
using Msv.AutoMiner.Rig.System.Windows;

namespace Msv.AutoMiner.Rig.System
{
    public class SystemStateProviderFactory : PlatformSpecificFactoryBase<ISystemStateProvider>
    {
        protected override ISystemStateProvider CreateForUnix()
            => new UnixSystemStateProvider();

        protected override ISystemStateProvider CreateForWindows()
            => new WindowsSystemStateProvider();
    }
}
