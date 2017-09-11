namespace Msv.AutoMiner.Service.Security.Contracts
{
    public interface IStringEncryptor
    {
        byte[] Encrypt(string str);
        string Decrypt(byte[] encrypted);
    }
}
