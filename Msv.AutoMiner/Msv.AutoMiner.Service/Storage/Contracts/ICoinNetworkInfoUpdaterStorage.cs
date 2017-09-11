using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.Storage.Contracts
{
    public interface ICoinNetworkInfoUpdaterStorage
    {
        Coin[] GetCoins();
        void SaveCoins(Coin[] coins);
    }
}