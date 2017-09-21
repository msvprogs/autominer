using System;
using System.Threading.Tasks;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Models.CoinInfoService;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.Common.ServiceContracts;

namespace Msv.AutoMiner.Rig.Remote
{
    public class ControlCenterServiceClient : IControlCenterService
    {
        private readonly IAsyncRestClient m_RestClient;

        public ControlCenterServiceClient(IAsyncRestClient restClient) 
            => m_RestClient = restClient ?? throw new ArgumentNullException(nameof(restClient));

        public Task<RegisterRigResponseModel> RegisterRig(RegisterRigRequestModel request)
            => m_RestClient.PostAsync<RegisterRigRequestModel, RegisterRigResponseModel>(
                "/api/controlCenter/registerRig",
                request);

        public Task<AlgorithmInfo[]> GetAlgorithms()
            => m_RestClient.GetAsync<AlgorithmInfo[]>("/api/controlCenter/getAlgorithms");

        public Task<SendHeartbeatResponseModel> SendHeartbeat(Heartbeat heartbeat)
            => m_RestClient.PostAsync<Heartbeat, SendHeartbeatResponseModel>(
                "/api/controlCenter/sendHeartbeat",
                heartbeat);

        public Task<MiningWorkModel[]> GetMiningWork(GetMiningWorkRequestModel request)
            => m_RestClient.PostAsync<GetMiningWorkRequestModel, MiningWorkModel[]>(
                "/api/controlCenter/getMiningWork",
                request);
    }
}