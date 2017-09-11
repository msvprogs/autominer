using Msv.AutoMiner.Commons;
using Msv.AutoMiner.Service.System.Contracts;
using Msv.AutoMiner.Service.System.Unix;
using Msv.AutoMiner.Service.System.Windows;

namespace Msv.AutoMiner.Service.System
{
    public class ProcessStopperFactory : PlatformSpecificFactoryBase<IProcessStopper>
    {
        protected override IProcessStopper CreateForUnix() => new UnixProcessStopper();
        protected override IProcessStopper CreateForWindows() => new WindowsProcessStopper();
    }
}
