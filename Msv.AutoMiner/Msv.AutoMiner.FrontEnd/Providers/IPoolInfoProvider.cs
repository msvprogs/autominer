using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.FrontEnd.Providers
{
    public interface IPoolInfoProvider
    {
        PoolAccountState[] GetCurrentPoolInfos();
    }
}