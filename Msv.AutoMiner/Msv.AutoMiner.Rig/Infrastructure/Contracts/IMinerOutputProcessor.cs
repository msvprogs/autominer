namespace Msv.AutoMiner.Rig.Infrastructure.Contracts
{
    public interface IMinerOutputProcessor
    {
        long CurrentHashRate { get; }
        long CurrentSecondaryHashRate { get; }
        int AcceptedShares { get; }
        int RejectedShares { get; }

        void Write(string output);
    }
}
