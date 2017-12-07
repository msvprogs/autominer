using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.FrontEnd.Providers
{
    public interface IWalletBalanceProvider
    {
        WalletBalance[] GetLastBalances();
    }
}
