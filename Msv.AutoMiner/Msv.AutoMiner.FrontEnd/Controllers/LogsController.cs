using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.ServiceContracts;
using Msv.AutoMiner.FrontEnd.Models.Logs;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class LogsController : Controller
    {
        private readonly ICoinInfoService m_CoinInfoService;
        private readonly IControlCenterService m_ControlCenterService;

        public LogsController(ICoinInfoService coinInfoService, IControlCenterService controlCenterService)
        {
            m_CoinInfoService = coinInfoService;
            m_ControlCenterService = controlCenterService;
        }

        public IActionResult Index() 
            => View();

        public async Task<ServiceLogsModel> GetLogs()
        {
            var coinInfoLogs = await m_CoinInfoService.GetLog();
            var controlCenterLogs = await m_ControlCenterService.GetLog();
            return new ServiceLogsModel
            {
                CoinInfoErrors = coinInfoLogs.Errors,
                CoinInfoFull = coinInfoLogs.Full,
                ControlCenterErrors = controlCenterLogs.Errors,
                ControlCenterFull = controlCenterLogs.Full
            };
        }
    }
}
