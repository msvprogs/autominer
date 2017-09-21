using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts
{
    public interface INetworkInfoMonitorStorage
    {
        Coin[] GetCoins();
        CoinNetworkInfo[] GetLastNetworkInfos();
        void StoreNetworkInfo(CoinNetworkInfo info);
    }
}