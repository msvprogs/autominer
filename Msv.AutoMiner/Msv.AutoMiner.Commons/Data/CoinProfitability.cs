using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Msv.AutoMiner.Commons.Data
{
    [Table("coinprofitabilities")]
    public class CoinProfitability
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CoinId { get; set; }

        public DateTime DateTime { get; set; }

        public double CoinsPerDay { get; set; }

        public double BtcPerDay { get; set; }

        public decimal UsdPerDay { get; set; }

        public virtual Coin Coin { get; set; }
    }
}