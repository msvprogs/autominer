using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.Data
{
    public class Wallet
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public Guid CoinId { get; set; }

        public DateTime Created { get; set; }

        public virtual Coin Coin { get; set; }

        public ActivityState Activity { get; set; }

        public ExchangeType? ExchangeType { get; set; }

        public string Address { get; set; }

        public bool IsMiningTarget { get; set; }
    }
}
