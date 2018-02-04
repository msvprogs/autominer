namespace Msv.AutoMiner.Rig.Infrastructure.Contracts
{
    public interface IMinerOutputProcessor : IMinerStatusProvider
    {
        void Write(string output);
    }
}
