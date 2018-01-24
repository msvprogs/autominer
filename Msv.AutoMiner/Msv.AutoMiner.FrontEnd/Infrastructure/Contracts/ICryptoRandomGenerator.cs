namespace Msv.AutoMiner.FrontEnd.Infrastructure.Contracts
{
    public interface ICryptoRandomGenerator
    {
        byte[] GenerateRandom(int bytes);
    }
}
