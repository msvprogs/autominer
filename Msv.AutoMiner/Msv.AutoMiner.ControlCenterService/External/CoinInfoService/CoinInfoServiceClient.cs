﻿using System;
using System.Threading.Tasks;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Models.CoinInfoService;
using Msv.AutoMiner.Common.ServiceContracts;

namespace Msv.AutoMiner.ControlCenterService.External.CoinInfoService
{
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

        public Task<ProfitabilityResponseModel> GetProfitabilities(ProfitabilityRequestModel request)
            => m_RestClient.PostAsync<ProfitabilityRequestModel, ProfitabilityResponseModel>(
                $"/api/coinInfo/getProfitabilities?apikey={m_ApiKey}", request);
    }
}