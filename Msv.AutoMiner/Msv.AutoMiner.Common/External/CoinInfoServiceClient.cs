using System;
using System.Reflection;
using System.Threading.Tasks;
using Msv.AutoMiner.Common.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Models.CoinInfoService;
using Msv.AutoMiner.Common.ServiceContracts;

namespace Msv.AutoMiner.Common.External
{
    [Obfuscation(Exclude = true)]
    public class CoinInfoServiceClient : ICoinInfoService
    {
        private readonly IAsyncRestClient m_RestClient;
        private readonly string m_ApiKey;

        public CoinInfoServiceClient(IAsyncRestClient restClient, string apiKey)
        {
            m_RestClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            m_ApiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        }

        public Task<AlgorithmInfo[]> GetAlgorithms() 
            => m_RestClient.GetAsync<AlgorithmInfo[]>($"/api/coinInfo/getAlgorithms?apikey={m_ApiKey}");

        public Task<ProfitabilityResponseModel> GetProfitabilities(ProfitabilityRequest request)
            => m_RestClient.PostAsync<ProfitabilityRequest, ProfitabilityResponseModel>(
                $"/api/coinInfo/getProfitabilities?apikey={m_ApiKey}", request);

        public Task<EstimateProfitabilityResponse> EstimateProfitability(EstimateProfitabilityRequest request)
            => m_RestClient.PostAsync<EstimateProfitabilityRequest, EstimateProfitabilityResponse>(
                $"/api/coinInfo/estimateProfitability?apikey={m_ApiKey}", request);

        public Task<ServiceLogs> GetLog()
            => m_RestClient.GetAsync<ServiceLogs>($"/api/coinInfo/getLog?apikey={m_ApiKey}");
    }
}