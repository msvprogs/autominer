using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Msv.AutoMiner.Commons.Data
{
    [Table("coinbtcprices")]
    public class CoinBtcPrice
    {
        [Key]
        [Column(Order = 0)]
        public int CoinId { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime DateTime { get; set; }

        public double HighestBid { get; set; }

        public double LowestAsk { get; set; }

        public virtual Coin Coin { get; set; }
    }
}
