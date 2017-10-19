using System;

namespace Msv.AutoMiner.Rig.Infrastructure.Contracts
{
    public interface IProcessWrapper : IDisposable
    {
        event EventHandler Exited;
        int Start();
        void Stop(bool forcefully);
    }
}
