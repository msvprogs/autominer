namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    public interface ICryptoRandomGenerator
    {
        byte[] GenerateRandom(int bytes);
    }
}
