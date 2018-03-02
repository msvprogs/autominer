using System.Diagnostics;

namespace Msv.AutoMiner.CoinInfoService.External.Data
{
    [DebuggerDisplay("{SourceSymbol} -> {TargetSymbol} Bid: {HighestBid} Ask: {LowestAsk} Last: {LastPrice} Vol: {LastDayVolume}")]
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
