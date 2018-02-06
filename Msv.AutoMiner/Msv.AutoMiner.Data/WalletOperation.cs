using System;
using System.ComponentModel.DataAnnotations;

namespace Msv.AutoMiner.Data
{
    public class WalletOperation
    {
        public int WalletId { get; set; }

        public virtual Wallet Wallet { get; set; }

        public DateTime DateTime { get; set; }

        [MaxLength(64)]
        public string ExternalId { get; set; }

        public double Amount { get; set; }

        [MaxLength(256)]
        public string TargetAddress { get; set; }

        [MaxLength(256)]
        public string Transaction { get; set; }
    }
}
