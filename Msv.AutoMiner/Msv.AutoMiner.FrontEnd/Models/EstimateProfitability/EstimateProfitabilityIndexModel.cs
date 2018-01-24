using Msv.AutoMiner.FrontEnd.Models.Algorithms;
using Msv.AutoMiner.FrontEnd.Models.Tools;

namespace Msv.AutoMiner.FrontEnd.Models.EstimateProfitability
{
    public class EstimateProfitabilityIndexModel
    {
        public CoinModel[] Coins { get; set; }
        public AlgorithmModel[] Algorithms { get; set; }
        public RigModel[] Rigs { get; set; }
        public double? ElectricityCostUsd { get; set; }
    }
}
