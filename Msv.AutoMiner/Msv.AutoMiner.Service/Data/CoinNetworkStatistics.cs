namespace Msv.AutoMiner.Service.Data
{
    public class CoinNetworkStatistics
    {
        public double Difficulty { get; set; }
        public long NetHashRate { get; set; }
        public long? Height { get; set; }
        public double? BlockTimeSeconds { get; set; }
        public double? BlockReward { get; set; }
    }
}
