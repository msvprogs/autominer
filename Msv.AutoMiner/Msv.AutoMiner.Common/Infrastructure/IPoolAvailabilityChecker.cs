using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.Models.ControlCenterService;

namespace Msv.AutoMiner.Common.Infrastructure
{
    public interface IPoolAvailabilityChecker
    {
        PoolAvailabilityState Check(PoolDataModel poolData, KnownCoinAlgorithm? knownCoinAlgorithm);
    }
}
