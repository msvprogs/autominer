﻿using System.Threading.Tasks;
using Msv.AutoMiner.Common.Data;
using Msv.AutoMiner.Common.Models.CoinInfoService;

namespace Msv.AutoMiner.Common.ServiceContracts
{
    public interface ICoinInfoService
    {
        Task<AlgorithmInfo[]> GetAlgorithms();
        Task<ProfitabilityResponseModel> GetProfitabilities(ProfitabilityRequestModel request);
        Task<EstimateProfitabilityResponseModel> EstimateProfitability(EstimateProfitabilityRequestModel request);
        Task<ServiceLogs> GetLog();
    }
}
