namespace Msv.AutoMiner.Common.Models.CoinInfoService
{
    public class EstimateProfitabilityResponse
    {
        public CumulativeProfitability Coins { get; set; }
        public CumulativeProfitability Btc { get; set; }
        public CumulativeProfitability Usd { get; set; }

        public class CumulativeProfitability
        {
            public double PerDay { get; set; }
            public double PerWeek { get; set; }
            public double PerMonth { get; set; }

            public CumulativeProfitability(double perDay)
            {
                PerDay = perDay;
                PerWeek = perDay * 7;
                PerMonth = perDay * 30;
            }
        }
    }
}
