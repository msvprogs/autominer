using System;

namespace Msv.AutoMiner.Service.Infrastructure.Contracts
{
    public interface IProcessWrapper : IDisposable
    {
        event EventHandler Exited;
        bool IsAlive { get; }
        int Start();
        void Stop(bool forcefully);
    }
}
