namespace Msv.AutoMiner.Data.Logic.Contracts
{
    public interface IPoolInfoProvider
    {
        PoolAccountState[] GetCurrentPoolInfos();
    }
}