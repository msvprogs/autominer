using Msv.AutoMiner.Common.Models.CoinInfoService;

namespace Msv.AutoMiner.Common.Models.ControlCenterService
{
    public class GetMiningWorkRequestModel
    {
        public AlgorithmPowerData[] AlgorithmDatas { get; set; }

        public double ElectricityCostUsd { get; set; }

        public bool TestMode { get; set; }
    }
}