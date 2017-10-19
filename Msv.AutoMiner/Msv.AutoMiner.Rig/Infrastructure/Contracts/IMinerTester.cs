namespace Msv.AutoMiner.Rig.Infrastructure.Contracts
{
    public interface IMinerTester
    {
        void Test(bool benchmarkMode, string[] algorithmNames);
    }
}
