namespace Msv.AutoMiner.Service.Data
{
    public class CoinMarketInfo
    {
        public string Symbol { get; set; }
        public double BtcHighestBid { get; set; }
        public double BtcLowestAsk { get; set; }
        public double LtcHighestBid { get; set; }
        public double LtcLowestAsk { get; set; }
        public double? WithdrawalFee { get; set; }
        public double ConversionFeePercent { get; set; }
    }
}
