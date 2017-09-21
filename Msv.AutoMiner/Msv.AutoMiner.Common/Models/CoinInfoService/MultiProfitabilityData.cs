namespace Msv.AutoMiner.Common.Models.CoinInfoService
{
    public class MultiProfitabilityData
    {
        public string[] CoinSymbols { get; set; }

        public double[] CoinsPerDay { get; set; }

        public double ElectricityCostPerDay { get; set; }

        public MarketPriceData[] MarketPrices { get; set; }
    }
}
