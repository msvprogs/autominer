namespace Msv.AutoMiner.FrontEnd.Infrastructure.Contracts
{
    public interface IPasswordHasher
    {
        byte[] CalculateHash(string password, byte[] salt);
    }
}
