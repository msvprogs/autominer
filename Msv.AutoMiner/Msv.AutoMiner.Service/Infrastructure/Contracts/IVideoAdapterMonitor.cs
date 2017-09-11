using System;
using Msv.AutoMiner.Service.Video;

namespace Msv.AutoMiner.Service.Infrastructure.Contracts
{
    public interface IVideoAdapterMonitor : IDisposable
    {
        bool AreAlive { get; }
        VideoSystemState GetCurrentState();
    }
}
