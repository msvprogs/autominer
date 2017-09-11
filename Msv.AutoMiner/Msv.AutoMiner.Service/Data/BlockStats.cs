namespace Msv.AutoMiner.Service.Data
{
    public struct BlockStats
    {
        public double MeanBlockTime { get; set; }
        public double? LastReward { get; set; }
        public long Height { get; set; }
    }
}
