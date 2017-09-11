using System;

namespace Msv.AutoMiner.Service.System.Contracts
{
    public interface IWatchdog : IDisposable
    {
        void Feed();
    }
}
