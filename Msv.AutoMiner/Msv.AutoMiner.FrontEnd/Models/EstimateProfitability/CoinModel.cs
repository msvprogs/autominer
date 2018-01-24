using Msv.AutoMiner.FrontEnd.Models.Coins;

namespace Msv.AutoMiner.FrontEnd.Models.Tools
{
    public class CoinModel : CoinBaseModel
    {
        public double? Difficulty { get; set; }
        public double? BlockReward { get; set; }
        public string MaxTarget { get; set; }
        public double? BtcPrice { get; set; }
    }
}
