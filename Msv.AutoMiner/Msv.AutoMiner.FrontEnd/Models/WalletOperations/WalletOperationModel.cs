﻿using System;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.FrontEnd.Models.WalletOperations
{
    public class WalletOperationModel
    {
        public string Id { get; set; }

        public double Amount { get; set; }

        public string CurrencySymbol { get; set; }

        public string CurrencyName { get; set; }

        public byte[] CurrencyLogo { get; set; }

        public ExchangeType? Exchange { get; set; }

        public DateTime DateTime { get; set; }

        public string Transaction { get; set; }

        public Uri TransactionUrl { get; set; }

        public string TargetAddress { get; set; }

        public Uri TargetAddressUrl { get; set; }
    }
}
