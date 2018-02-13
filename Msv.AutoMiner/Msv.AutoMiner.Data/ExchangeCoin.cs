using System;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.Data
{
    public class ExchangeCoin
    {
        public ExchangeType Exchange { get; set; }

        public Guid CoinId { get; set; }

        public virtual Coin Coin { get; set; }

        public DateTime DateTime { get; set; }

        public bool IsActive { get; set; }

        public double MinWithdrawAmount { get; set; }

        public double WithdrawalFee { get; set; }
    }
}
