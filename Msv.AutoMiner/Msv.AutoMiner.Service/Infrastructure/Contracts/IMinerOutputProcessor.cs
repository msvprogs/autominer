namespace Msv.AutoMiner.Service.Infrastructure.Contracts
{
    public interface IMinerOutputProcessor
    {
        long CurrentHashRate { get; }
        long CurrentSecondaryHashRate { get; }
        int? AcceptedShares { get; }

        void Write(string output);
    }
}
