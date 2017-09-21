namespace Msv.AutoMiner.CoinInfoService.External.Data
{
    public class CurrencyMarketInfo
    {
        public string SourceSymbol { get; set; }
        public string TargetSymbol { get; set; }
        public bool IsActive { get; set; }
        public double HighestBid { get; set; }
        public double LowestAsk { get; set; }
        public double LastPrice { get; set; }
        public double LastDayVolume { get; set; }
        public double LastDayHigh { get; set; }
        public double LastDayLow { get; set; }
        public double BuyFeePercent { get; set; }
        public double SellFeePercent { get; set; }
    }
}
