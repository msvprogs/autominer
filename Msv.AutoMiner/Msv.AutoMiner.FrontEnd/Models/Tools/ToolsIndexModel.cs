using Msv.AutoMiner.FrontEnd.Models.Algorithms;

namespace Msv.AutoMiner.FrontEnd.Models.Tools
{
    public class ToolsIndexModel
    {
        public CoinModel[] Coins { get; set; }
        public AlgorithmModel[] Algorithms { get; set; }
        public RigModel[] Rigs { get; set; }
        public double? ElectricityCostUsd { get; set; }
    }
}
