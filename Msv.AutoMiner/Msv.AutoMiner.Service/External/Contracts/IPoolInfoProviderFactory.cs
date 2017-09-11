using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.External.Contracts
{
    public interface IPoolInfoProviderFactory
    {
        IPoolInfoProvider Create(Coin coin, IDDoSTriggerPreventingDownloader downloader);
    }
}
