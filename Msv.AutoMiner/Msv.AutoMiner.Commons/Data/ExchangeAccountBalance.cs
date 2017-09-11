using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Msv.AutoMiner.Commons.Data
{
    [Table("exchangeaccountbalances")]
    public class ExchangeAccountBalance
    {
        [Key]
        [Column(Order = 0)]
        public int CoinId { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime DateTime { get; set; }

        [Key]
        [Column(Order = 2)]
        public ExchangeType Exchange { get; set; }

        public double Balance { get; set; }

        public double PendingBalance { get; set; }

        public double BalanceOnOrders { get; set; }

        public virtual Coin Coin { get; set; }
    }
}
