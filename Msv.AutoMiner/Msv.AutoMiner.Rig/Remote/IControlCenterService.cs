using System.IO;
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
        CheckConfigurationResponseModel CheckConfiguration(GetConfigurationRequestModel request);
        GetConfigurationResponseModel GetConfiguration(GetConfigurationRequestModel request);
        Stream DownloadMiner(int versionId);
    }
}