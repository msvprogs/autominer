using Msv.AutoMiner.Common.Models.CoinInfoService;
using Msv.AutoMiner.Common.Models.ControlCenterService;

namespace Msv.AutoMiner.Rig.Remote
{
    public interface IControlCenterService
    {
        RegisterRigResponseModel RegisterRig(RegisterRigRequestModel request);
        AlgorithmInfo[] GetAlgorithms();
        SendHeartbeatResponseModel SendHeartbeat(Heartbeat heartbeat);
        MiningWorkModel[] GetMiningWork(GetMiningWorkRequestModel request);
    }
}