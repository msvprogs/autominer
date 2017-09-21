namespace Msv.AutoMiner.CoinInfoService.External.Data
{
    public struct BlockStats
    {
        public double MeanBlockTime { get; set; }
        public double? LastReward { get; set; }
        public long Height { get; set; }
    }
}
