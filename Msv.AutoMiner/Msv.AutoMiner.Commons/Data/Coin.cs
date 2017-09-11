using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Msv.AutoMiner.Commons.Data
{
    [Table("coins")]
    public class Coin
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ActivityState Activity { get; set; }

        [Required]
        public string CurrencySymbol { get; set; }

        [Required]
        public CoinAlgorithm Algorithm { get; set; }

        public double Difficulty { get; set; }

        public long NetHashRate { get; set; }

        public double BlockReward { get; set; }

        public double BlockTimeSeconds { get; set; }

        public long Height { get; set; }

        public DateTime? StatsUpdated { get; set; }

        public ExchangeType Exchange { get; set; }

        [Required]
        public string Wallet { get; set; }

        public int? PoolId { get; set; }

        public bool ProfitByAskPrice { get; set; }

        public int? SolsPerDiff { get; set; }

        public bool UseLocalNetworkInfo { get; set; }

        public virtual Pool Pool { get; set; }
    }
}
