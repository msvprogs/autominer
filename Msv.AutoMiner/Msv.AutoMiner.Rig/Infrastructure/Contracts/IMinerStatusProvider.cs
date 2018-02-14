using System;

namespace Msv.AutoMiner.Rig.Infrastructure.Contracts
{
    public interface IMinerStatusProvider : IDisposable
    {
        double CurrentHashRate { get; }
        int AcceptedShares { get; }
        int RejectedShares { get; }
    }
}
