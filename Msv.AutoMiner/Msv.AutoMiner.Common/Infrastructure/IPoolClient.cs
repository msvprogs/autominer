using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.Common.Infrastructure
{
    public interface IPoolClient
    {
        PoolAvailabilityState CheckAvailability();
    }
}
