using Msv.AutoMiner.Commons;
using Msv.AutoMiner.Service.System.Contracts;
using Msv.AutoMiner.Service.System.Unix;
using Msv.AutoMiner.Service.System.Windows;

namespace Msv.AutoMiner.Service.System
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
