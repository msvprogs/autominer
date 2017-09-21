namespace Msv.AutoMiner.ControlCenterService.Security.Contracts
{
    public interface IStringEncryptor
    {
        byte[] Encrypt(string str);
        string Decrypt(byte[] encrypted);
    }
}
