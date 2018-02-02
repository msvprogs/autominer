namespace Msv.AutoMiner.FrontEnd.Infrastructure.Contracts
{
    public interface IWalletAddressValidator
    {
        bool IsValid(string address);
    }
}
