using System;
using Msv.Licensing.Client.Contracts;

namespace Msv.Licensing.Client
{
    internal class HardwareDataProviderFactory : IHardwareDataProviderFactory
    {
        public IHardwareDataProvider Create()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    return new WindowsHardwareDataProvider();
                case PlatformID.Unix:
                    return new LinuxHardwareDataProvider();
                default:
                    throw new PlatformNotSupportedException();
            }
        }
    }
}
