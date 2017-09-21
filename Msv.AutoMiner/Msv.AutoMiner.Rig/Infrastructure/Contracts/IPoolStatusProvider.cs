using Msv.AutoMiner.Common.Models.ControlCenterService;

namespace Msv.AutoMiner.Rig.Infrastructure.Contracts
{
    public interface IPoolStatusProvider
    {
        bool CheckAvailability(PoolDataModel poolData);
    }
}
