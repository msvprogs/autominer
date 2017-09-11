using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.External.Contracts
{
    public interface ICoinNetworkInfoProviderFactory
    {
        ICoinNetworkInfoProvider Create(Coin coin);
        IMultiCoinNetworkInfoProvider CreateMulti(Coin[] coins, IDDoSTriggerPreventingDownloader downloader);
    }
}
