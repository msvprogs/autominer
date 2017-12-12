namespace Msv.AutoMiner.Data.Logic
{
    public interface IPoolInfoProvider
    {
        PoolAccountState[] GetCurrentPoolInfos();
    }
}