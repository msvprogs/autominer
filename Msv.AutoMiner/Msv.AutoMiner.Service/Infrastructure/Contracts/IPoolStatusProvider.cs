namespace Msv.AutoMiner.Service.Infrastructure.Contracts
{
    public interface IPoolStatusProvider
    {
        bool CheckAvailability(int poolId);
    }
}
