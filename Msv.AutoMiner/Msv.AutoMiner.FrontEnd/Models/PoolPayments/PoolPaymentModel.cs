using System;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.FrontEnd.Models.PoolPayments
{
    public class PoolPaymentModel
    {
        public string Id { get; set; }

        public string PoolName { get; set; }

        public DateTime DateTime { get; set; }

        public string CurrencyName { get; set; }

        public string CurrencySymbol { get; set; }

        public byte[] CurrencyLogo { get; set; }

        public double Amount { get; set; }

        public string Transaction { get; set; }

        public string BlockHash { get; set; }

        public string Address { get; set; }

        public PoolPaymentType Type { get; set; }
    }
}