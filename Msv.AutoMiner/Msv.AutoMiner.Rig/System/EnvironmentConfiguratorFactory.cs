using Msv.AutoMiner.Rig.System.Contracts;
using Msv.AutoMiner.Rig.System.Unix;
using Msv.AutoMiner.Rig.System.Windows;

namespace Msv.AutoMiner.Rig.System
{
    public class EnvironmentConfiguratorFactory : PlatformSpecificFactoryBase<IEnvironmentConfigurator>
    {
        protected override IEnvironmentConfigurator CreateForUnix()
            => new MonoEnvironmentConfigurator();

        protected override IEnvironmentConfigurator CreateForWindows()
            => new DummyEnvironmentConfigurator();
    }
}
