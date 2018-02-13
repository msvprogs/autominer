namespace Msv.AutoMiner.Common.Data
{
    public interface IPasswordHasher
    {
        byte[] CalculateHash(string password, byte[] salt);
    }
}
