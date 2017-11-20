using Msv.AutoMiner.Common.Infrastructure;

namespace Msv.AutoMiner.Common.Models.CoinInfoService
{
    public class EstimateProfitabilityRequestModel
    {
        public double Difficulty { get; set; }

        public double BlockReward { get; set; }

        [HexNumber(ErrorMessage = "Invalid hex number")]
        public string MaxTarget { get; set; }

        public double BtcPrice { get; set; }

        public double ClientHashRate { get; set; }

        public double ClientPowerUsage { get; set; }

        public double ElectricityCostUsd { get; set; }
    }
}
