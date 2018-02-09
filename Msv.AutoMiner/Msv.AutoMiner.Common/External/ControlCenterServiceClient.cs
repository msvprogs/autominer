using System;
using System.Threading.Tasks;
using Msv.AutoMiner.Common.Data;
using Msv.AutoMiner.Common.External.Contracts;

namespace Msv.AutoMiner.Common.External
{
    public class ControlCenterServiceClient : IControlCenterService
    {
        private readonly IAsyncRestClient m_RestClient;

        public ControlCenterServiceClient(IAsyncRestClient restClient)
        {
            m_RestClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        public Task<ServiceLogs> GetLog()
            => m_RestClient.GetAsync<ServiceLogs>("/api/controlCenter/getLog");
    }
}
