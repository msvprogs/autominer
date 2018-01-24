using Msv.AutoMiner.Common.Models.CoinInfoService;

namespace Msv.AutoMiner.FrontEnd.Models.Tools
{
    public class RigModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public AlgorithmPowerData[] HashRates { get; set; }
    }
}
