using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Msv.AutoMiner.Commons.Data
{
    [Table("miningchangeevents")]
    public class MiningChangeEvent
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime DateTime { get; set; }

        public int? FromCoinId { get; set; }

        public int ToCoinId { get; set; }

        public decimal? SourceProfitability { get; set; }

        public decimal TargetProfitability { get; set; }

        public virtual Coin FromCoin { get; set; }

        public virtual Coin ToCoin { get; set; }
    }
}
