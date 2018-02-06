using System;
using System.ComponentModel.DataAnnotations;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.Data
{
    public class PoolPayment
    {
        public int PoolId { get; set; }

        public virtual Pool Pool { get; set; }

        [MaxLength(64)]
        public string ExternalId { get; set; }

        public DateTime DateTime { get; set; }

        public double Amount { get; set; }

        [MaxLength(256)]
        public string Transaction { get; set; }

        [MaxLength(256)]
        public string BlockHash { get; set; }

        [MaxLength(256)]
        public string CoinAddress { get; set; }

        public int? WalletId { get; set; }

        public Wallet Wallet { get; set; }

        public PoolPaymentType Type { get; set; }
    }
}
