using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.Data
{
    public class ExchangeCurrency
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public ExchangeType Exchange { get; set; }

        [Required, MaxLength(16)]
        public string Symbol { get; set; }

        public Guid? CoinId { get; set; }

        public virtual Coin Coin { get; set; }

        public DateTime DateTime { get; set; }

        public bool IsActive { get; set; }

        public double MinWithdrawAmount { get; set; }

        public double WithdrawalFee { get; set; }
    }
}
