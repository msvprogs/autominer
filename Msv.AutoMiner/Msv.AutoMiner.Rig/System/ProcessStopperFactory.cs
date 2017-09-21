using Msv.AutoMiner.Rig.System.Contracts;
using Msv.AutoMiner.Rig.System.Unix;
using Msv.AutoMiner.Rig.System.Windows;

namespace Msv.AutoMiner.Rig.System
{
    public class ProcessStopperFactory : PlatformSpecificFactoryBase<IProcessStopper>
    {
        protected override IProcessStopper CreateForUnix() => new UnixProcessStopper();
        protected override IProcessStopper CreateForWindows() => new WindowsProcessStopper();
    }
}
