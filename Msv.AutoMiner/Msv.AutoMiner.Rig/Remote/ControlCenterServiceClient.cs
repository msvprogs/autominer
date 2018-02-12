using System;
using System.IO;
using Msv.AutoMiner.Common.Licensing;
using Msv.AutoMiner.Common.Models.ControlCenterService;

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
                AddLicenseId(request));

        public SendHeartbeatResponseModel SendHeartbeat(Heartbeat heartbeat)
            => m_RestClient.Post<Heartbeat, SendHeartbeatResponseModel>(
                "/api/controlCenter/sendHeartbeat",
                heartbeat);

        public MiningWorkModel[] GetMiningWork(GetMiningWorkRequestModel request)
            => m_RestClient.Post<GetMiningWorkRequestModel, MiningWorkModel[]>(
                "/api/controlCenter/getMiningWork",
                AddLicenseId(request));

        public CheckConfigurationResponseModel CheckConfiguration(GetConfigurationRequestModel request)
            => m_RestClient.Post<GetConfigurationRequestModel, CheckConfigurationResponseModel>(
                "/api/controlCenter/checkConfiguration",
                AddLicenseId(request));

        public GetConfigurationResponseModel GetConfiguration(GetConfigurationRequestModel request)
            => m_RestClient.Post<GetConfigurationRequestModel, GetConfigurationResponseModel>(
                "/api/controlCenter/getConfiguration",
                AddLicenseId(request));

        public Stream DownloadMiner(int versionId)
            => m_RestClient.GetStream($"/api/controlCenter/downloadMiner/{versionId}");

        private static T AddLicenseId<T>(T request)
            where T : LicensedRequestBase
        {
            request.LicenseId = LicenseData.Current.LicenseId;
            return request;
        }
    }
}