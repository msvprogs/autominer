namespace Msv.AutoMiner.Common.Infrastructure
{
    public interface IPasswordHasher
    {
        byte[] CalculateHash(string password, byte[] salt);
    }
}
