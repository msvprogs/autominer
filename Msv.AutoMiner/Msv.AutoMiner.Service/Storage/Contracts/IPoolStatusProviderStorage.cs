using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.Storage.Contracts
{
    public interface IPoolStatusProviderStorage
    {
        Pool GetPool(int poolId);
        void SavePool(Pool pool);
    }
}
