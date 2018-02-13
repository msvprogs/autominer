using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.CoinInfoService.Storage;
using Msv.AutoMiner.Common.Data;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.Log;
using Msv.AutoMiner.Common.Models.CoinInfoService;
using Msv.AutoMiner.Common.ServiceContracts;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.Data.Logic.Contracts;

namespace Msv.AutoMiner.CoinInfoService.Controllers
{
    [Route("api/[controller]")]
    public class CoinInfoController : Controller, ICoinInfoService
    {
        private readonly IProfitabilityTableBuilder m_TableBuilder;
        private readonly ICoinInfoControllerStorage m_Storage;

        public CoinInfoController(IProfitabilityTableBuilder tableBuilder, ICoinInfoControllerStorage storage)
        {
            m_TableBuilder = tableBuilder;
            m_Storage = storage;
        }

        [HttpGet("getAlgorithms")]
        //[ValidateApiKey(ApiKeyType.CoinInfoService, false)]
        public Task<AlgorithmInfo[]> GetAlgorithms() 
            => Task.FromResult(m_Storage.GetAlgorithms()
                .Where(x => x.Activity == ActivityState.Active)
                .Select(x => new AlgorithmInfo
                {
                    Id = x.Id,
                    KnownValue = x.KnownValue,
                    Name = x.Name
                })
                .ToArray());

        [HttpPost("getProfitabilities")]
        //[ValidateApiKey(ApiKeyType.CoinInfoService)]
        public Task<ProfitabilityResponseModel> GetProfitabilities([FromBody] ProfitabilityRequest request)
            => Task.FromResult(new ProfitabilityResponseModel
            {
                Profitabilities = m_TableBuilder.Build(request)
            });

        [HttpPost("estimateProfitability")]
        //[ValidateApiKey(ApiKeyType.CoinInfoService)]
        public Task<EstimateProfitabilityResponse> EstimateProfitability(
            [FromBody] EstimateProfitabilityRequest request) 
            => Task.FromResult(m_TableBuilder.EstimateProfitability(request));

        [HttpGet("getLog")]
        //[ValidateApiKey(ApiKeyType.CoinInfoService)] // ONLY FOR INTERNAL SERVICE!!!!!!!!
        public Task<ServiceLogs> GetLog()
            => Task.FromResult(new ServiceLogs
            {
                Errors = MemoryBufferTarget.GetBuffer("ErrorLogBuffer"),
                Full = MemoryBufferTarget.GetBuffer("FullLogBuffer")
            });
    }
}