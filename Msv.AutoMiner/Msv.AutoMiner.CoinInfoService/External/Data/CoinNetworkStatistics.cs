namespace Msv.AutoMiner.CoinInfoService.External.Data
{
    public class CoinNetworkStatistics
    {
        public double Difficulty { get; set; }
        public double NetHashRate { get; set; }
        public long Height { get; set; }
        public double? BlockTimeSeconds { get; set; }
        public double? BlockReward { get; set; }
        public double? MoneySupply { get; set; }
        public int? MasternodeCount { get; set; }
    }
}
