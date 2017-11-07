namespace Msv.AutoMiner.Rig.Infrastructure.Contracts
{
    public interface IMinerTester
    {
        void Test(string[] algorithmNames, string[] coinNames);
    }
}
