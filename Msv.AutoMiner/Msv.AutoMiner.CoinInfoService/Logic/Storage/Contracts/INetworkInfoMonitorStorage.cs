using System;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts
{
    public interface INetworkInfoMonitorStorage
    {
        Coin[] GetCoins();
        void StoreNetworkInfo(CoinNetworkInfo info);
        void StoreCoinNetworkResult(Guid coinId, CoinLastNetworkInfoResult result, string message);
    }
}