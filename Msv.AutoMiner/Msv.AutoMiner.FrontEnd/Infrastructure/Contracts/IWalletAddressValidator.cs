namespace Msv.AutoMiner.FrontEnd.Infrastructure.Contracts
{
    public interface IWalletAddressValidator
    {
        bool HasCheckSum(string address);
        bool IsValid(string address);
    }
}
