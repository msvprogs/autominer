using System;
using Msv.AutoMiner.Rig.System.Video;

namespace Msv.AutoMiner.Rig.Infrastructure.Contracts
{
    public interface IVideoAdapterMonitor : IDisposable
    {
        bool IsAlive { get; }
        VideoSystemState GetCurrentState();
    }
}
