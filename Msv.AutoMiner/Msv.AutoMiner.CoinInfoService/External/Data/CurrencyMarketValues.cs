namespace Msv.AutoMiner.CoinInfoService.External.Data
{
    public class CurrencyMarketValues : ExchangeCurrencyInfo
    {
        public double BtcHighestBid { get; set; }
        public double BtcLowestAsk { get; set; }
        public double LtcHighestBid { get; set; }
        public double LtcLowestAsk { get; set; }
    }
}