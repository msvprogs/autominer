using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.NetworkInfo
{
    public interface INetworkInfoProviderFactory
    {
        INetworkInfoProvider Create(Coin coin);
        IMultiNetworkInfoProvider CreateMulti(Coin[] coins);
    }
}
