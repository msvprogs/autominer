using System;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.FrontEnd.Models.Exchanges
{
    public class ExchangeModel
    {
        public ExchangeType Type { get; set; }

        public ActivityState Activity { get; set; }

        public bool HasKeys { get; set; }

        public int WalletCount { get; set; }

        public DateTime? LastPriceDate { get; set; }

        public DateTime? LastBalanceDate { get; set; }
    }
}
