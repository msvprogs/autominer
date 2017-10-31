using System;

namespace Msv.AutoMiner.FrontEnd.Models.PoolPayments
{
    public class PoolPaymentModel
    {
        public string Id { get; set; }
        public string PoolName { get; set; }
        public DateTime DateTime { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencySymbol { get; set; }
        public double Amount { get; set; }
        public string Transaction { get; set; }
    }
}