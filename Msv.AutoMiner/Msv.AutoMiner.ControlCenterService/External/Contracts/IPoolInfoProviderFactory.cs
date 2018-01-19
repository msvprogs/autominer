using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.External.Contracts
{
    public interface IPoolInfoProviderFactory
    {
        IMultiPoolInfoProvider CreateMulti(PoolApiProtocol apiProtocol, string baseUrl, Pool[] pools, Wallet btcMiningTarget);
        IPoolInfoProvider Create(Pool pool, Wallet btcMiningTarget);
    }
}
