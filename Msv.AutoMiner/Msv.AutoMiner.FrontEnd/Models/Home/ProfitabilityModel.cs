namespace Msv.AutoMiner.FrontEnd.Models.Home
{
    public class ProfitabilityModel
    {
        public string CoinName { get; set; }
        public byte[] CoinLogo { get; set; }
        public string PoolName { get; set; }
        public double CoinsPerDay { get; set; }
        public double BtcPerDay { get; set; }
        public double UsdPerDay { get; set; }
    }
}
