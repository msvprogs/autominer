using System.Threading.Tasks;
using Msv.AutoMiner.Common.Models.CoinInfoService;
using Msv.AutoMiner.Common.Models.ControlCenterService;

namespace Msv.AutoMiner.Common.ServiceContracts
{
    public interface IControlCenterService
    {
        Task<RegisterRigResponseModel> RegisterRig(RegisterRigRequestModel request);
        Task<AlgorithmInfo[]> GetAlgorithms();
        Task<SendHeartbeatResponseModel> SendHeartbeat(Heartbeat heartbeat);
        Task<MiningWorkModel[]> GetMiningWork(GetMiningWorkRequestModel request);
    }
}