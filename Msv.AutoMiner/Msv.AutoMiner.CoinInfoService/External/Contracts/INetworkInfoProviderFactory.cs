using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.External.Contracts
{
    public interface INetworkInfoProviderFactory
    {
        INetworkInfoProvider Create(Coin coin);
        IMultiNetworkInfoProvider CreateMulti(Coin[] coins);
    }
}
