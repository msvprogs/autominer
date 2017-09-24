using System;
using Msv.AutoMiner.Common.Models.CoinInfoService;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.Common.ServiceContracts;

namespace Msv.AutoMiner.Rig.Remote
{
    public class ControlCenterServiceClient : IControlCenterService
    {
        private readonly IRestClient m_RestClient;

        public ControlCenterServiceClient(IRestClient restClient) 
            => m_RestClient = restClient ?? throw new ArgumentNullException(nameof(restClient));

        public RegisterRigResponseModel RegisterRig(RegisterRigRequestModel request)
            => m_RestClient.Post<RegisterRigRequestModel, RegisterRigResponseModel>(
                "/api/controlCenter/registerRig",
                request);

        public AlgorithmInfo[] GetAlgorithms()
            => m_RestClient.Get<AlgorithmInfo[]>("/api/controlCenter/getAlgorithms");

        public SendHeartbeatResponseModel SendHeartbeat(Heartbeat heartbeat)
            => m_RestClient.Post<Heartbeat, SendHeartbeatResponseModel>(
                "/api/controlCenter/sendHeartbeat",
                heartbeat);

        public MiningWorkModel[] GetMiningWork(GetMiningWorkRequestModel request)
            => m_RestClient.Post<GetMiningWorkRequestModel, MiningWorkModel[]>(
                "/api/controlCenter/getMiningWork",
                request);
    }
}