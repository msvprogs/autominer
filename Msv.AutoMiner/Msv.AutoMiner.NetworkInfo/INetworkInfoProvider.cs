namespace Msv.AutoMiner.NetworkInfo
{
    public interface INetworkInfoProvider : IBlockExplorerUrlProvider
    {
        CoinNetworkStatistics GetNetworkStats();
    }
}
