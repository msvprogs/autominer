using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.ServiceContracts;
using Msv.AutoMiner.FrontEnd.Models;
using Msv.AutoMiner.FrontEnd.Models.Home;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICoinInfoService m_CoinInfoService;
        private readonly IControlCenterService m_ControlCenterService;

        public HomeController(ICoinInfoService coinInfoService, IControlCenterService controlCenterService)
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

        public IActionResult Error()
            => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
