using System.Reflection;
using System.Threading.Tasks;
using Msv.AutoMiner.Common.Data;
using Msv.AutoMiner.Common.Models.CoinInfoService;

namespace Msv.AutoMiner.Common.ServiceContracts
{
    [Obfuscation(Exclude = true)]
    public interface ICoinInfoService
    {
        Task<AlgorithmInfo[]> GetAlgorithms();
        Task<ProfitabilityResponseModel> GetProfitabilities(ProfitabilityRequest request);
        Task<EstimateProfitabilityResponse> EstimateProfitability(EstimateProfitabilityRequest request);
        Task<ServiceLogs> GetLog();
    }
}
