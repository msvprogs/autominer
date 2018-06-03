using System;

namespace Msv.AutoMiner.NetworkInfo.Data
{
    public class CoinNetworkStatistics
    {
        public double Difficulty { get; set; }
        public double NetHashRate { get; set; }
        public long Height { get; set; }
        public double? BlockTimeSeconds { get; set; }
        public double? BlockReward { get; set; }
        public double? TotalSupply { get; set; }
        public int? MasternodeCount { get; set; }
        public DateTime? LastBlockTime { get; set; }
        public TransactionInfo[] LastBlockTransactions { get; set; }
    }
}
