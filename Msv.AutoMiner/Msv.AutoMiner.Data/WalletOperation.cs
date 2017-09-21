using System;

namespace Msv.AutoMiner.Data
{
    public class WalletOperation
    {
        public int WalletId { get; set; }

        public DateTime DateTime { get; set; }

        public string ExternalId { get; set; }

        public double Amount { get; set; }

        public string TargetAddress { get; set; }

        public string Transaction { get; set; }
    }
}
