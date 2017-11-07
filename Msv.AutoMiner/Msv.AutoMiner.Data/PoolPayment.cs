using System;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.Data
{
    public class PoolPayment
    {
        public int PoolId { get; set; }

        public virtual Pool Pool { get; set; }

        public string ExternalId { get; set; }

        public DateTime DateTime { get; set; }

        public double Amount { get; set; }

        public string Transaction { get; set; }

        public string BlockHash { get; set; }

        public string CoinAddress { get; set; }

        public int? WalletId { get; set; }

        public Wallet Wallet { get; set; }

        public PoolPaymentType Type { get; set; }
    }
}
