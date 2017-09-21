using Msv.AutoMiner.Rig.System.Contracts;
using Msv.AutoMiner.Rig.System.Unix;
using Msv.AutoMiner.Rig.System.Windows;

namespace Msv.AutoMiner.Rig.System
{
    public class EnvironmentVariableCreatorFactory : PlatformSpecificFactoryBase<IEnvironmentVariableCreator>
    {
        private readonly string m_UnixCudaLibraryPath;

        public EnvironmentVariableCreatorFactory(string unixCudaLibraryPath)
        {
            m_UnixCudaLibraryPath = unixCudaLibraryPath;
        }

        protected override IEnvironmentVariableCreator CreateForUnix() => new UnixEnvironmentVariableCreator(m_UnixCudaLibraryPath);
        protected override IEnvironmentVariableCreator CreateForWindows() => new StandardEnvironmentVariableCreator();
    }
}
