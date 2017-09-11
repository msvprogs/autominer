using System;

namespace Msv.AutoMiner.Service.Data
{
    public class ExchangeAccountOperationData
    {
        public string CurrencySymbol { get; set; }
        public double Amount { get; set; }
        public DateTime DateTime { get; set; }
    }
}
