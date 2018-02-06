namespace Msv.AutoMiner.Common.Infrastructure
{
    public interface ICryptoRandomGenerator
    {
        byte[] GenerateRandom(int bytes);
    }
}
