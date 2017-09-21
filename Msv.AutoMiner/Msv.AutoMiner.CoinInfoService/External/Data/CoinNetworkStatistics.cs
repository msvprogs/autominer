namespace Msv.AutoMiner.CoinInfoService.External.Data
{
    public class CoinNetworkStatistics
    {
        public double Difficulty { get; set; }
        public long NetHashRate { get; set; }
        public long Height { get; set; }
        public double? BlockTimeSeconds { get; set; }
        public double? BlockReward { get; set; }
    }
}
