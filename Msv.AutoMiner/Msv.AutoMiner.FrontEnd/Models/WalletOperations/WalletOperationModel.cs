using System;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.FrontEnd.Models.WalletOperations
{
    public class WalletOperationModel
    {
        public string Id { get; set; }

        public double Amount { get; set; }

        public string CurrencySymbol { get; set; }

        public string CurrencyName { get; set; }

        public ExchangeType? Exchange { get; set; }

        public DateTime DateTime { get; set; }

        public string Transaction { get; set; }

        public string TargetAddress { get; set; }
    }
}
