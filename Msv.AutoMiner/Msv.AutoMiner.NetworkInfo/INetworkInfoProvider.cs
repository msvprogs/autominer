using System;
using Msv.AutoMiner.NetworkInfo.Data;

namespace Msv.AutoMiner.NetworkInfo
{
    public interface INetworkInfoProvider : IBlockExplorerUrlProvider
    {
        CoinNetworkStatistics GetNetworkStats();
        WalletBalance GetWalletBalance(string address);
        BlockExplorerWalletOperation[] GetWalletOperations(string address, DateTime startDate);
    }
}
