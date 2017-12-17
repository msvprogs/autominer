namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    public interface IPasswordHasher
    {
        byte[] CalculateHash(string password, byte[] salt);
    }
}
