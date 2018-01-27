using Msv.AutoMiner.Common.Models.CoinInfoService;
using Msv.AutoMiner.FrontEnd.Models.Algorithms;

namespace Msv.AutoMiner.FrontEnd.Models.Home
{
    public class HomeIndexModel
    {
        public AlgorithmBaseModel[] Algorithms { get; set; }
        public AlgorithmPowerDataCost[] TotalAlgorithmCapabilities { get; set; }
        public ProfitabilityModel[] CurrentProfitabilities { get; set; }
    }
}
