using Msv.AutoMiner.Rig.System.Contracts;

namespace Msv.AutoMiner.Rig.System.Windows
{
    public class DummyEnvironmentConfigurator : IEnvironmentConfigurator
    {
        public string Check() 
            => null;

        public void Configure()
        { }
    }
}
