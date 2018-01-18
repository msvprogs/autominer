namespace Msv.AutoMiner.FrontEnd.Models.Coins
{
    public class CoinsIndexModel
    {
        public double BtcUsdRate { get; set; }
        public double BtcUsdRateDelta { get; set; }
        public CoinDisplayModel[] Coins { get; set; }
    }
}
