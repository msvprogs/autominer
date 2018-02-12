using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.Infrastructure;

namespace Msv.AutoMiner.Common.Models.CoinInfoService
{
    public class EstimateProfitabilityRequest
    {
        public KnownCoinAlgorithm? KnownAlgorithm { get; set; }

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
