using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Msv.AutoMiner.Commons.Data
{
    [Table("exchangeaccountoperations")]
    public class ExchangeAccountOperation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime DateTime { get; set; }

        public ExchangeType Exchange { get; set; }

        public int CoinId { get; set; }

        public double Amount { get; set; }

        public virtual Coin Coin { get; set; }
    }
}
