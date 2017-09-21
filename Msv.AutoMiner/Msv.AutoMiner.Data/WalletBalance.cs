using System;

namespace Msv.AutoMiner.Data
{
    public class WalletBalance
    {
        public int WalletId { get; set; }

        public DateTime DateTime { get; set; }

        public double Balance { get; set; }

        public double UnconfirmedBalance { get; set; }

        public double BlockedBalance { get; set; }
    }
}
