using System;

namespace Msv.AutoMiner.Rig.Infrastructure.Contracts
{
    public interface IMinerStatusProvider : IDisposable
    {
        long CurrentHashRate { get; }
        long CurrentSecondaryHashRate { get; }
        int AcceptedShares { get; }
        int RejectedShares { get; }
    }
}
