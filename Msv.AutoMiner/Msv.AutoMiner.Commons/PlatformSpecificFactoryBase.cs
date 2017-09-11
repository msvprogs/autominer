using System;

namespace Msv.AutoMiner.Commons
{
    public abstract class PlatformSpecificFactoryBase<T>
    {
        public T Create()
        {
            var platform = Environment.OSVersion.Platform;
            switch (platform)
            {
                case PlatformID.Win32NT:
                    return CreateForWindows();
                case PlatformID.Unix:
                    return CreateForUnix();
                default:
                    throw new NotSupportedException($"Platform {platform} is not supported");
            }
        }

        protected abstract T CreateForUnix();
        protected abstract T CreateForWindows();
    }
}
