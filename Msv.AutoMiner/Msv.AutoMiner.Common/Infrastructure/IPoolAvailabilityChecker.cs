using Msv.AutoMiner.Common.Models.ControlCenterService;

namespace Msv.AutoMiner.Common.Infrastructure
{
    public interface IPoolAvailabilityChecker
    {
        bool Check(PoolDataModel poolData);
    }
}
