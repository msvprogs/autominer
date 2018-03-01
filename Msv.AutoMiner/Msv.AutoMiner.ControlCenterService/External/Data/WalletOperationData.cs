using System;
using System.Diagnostics;

namespace Msv.AutoMiner.ControlCenterService.External.Data
{
    [DebuggerDisplay("{Amount} {CurrencySymbol} at {DateTime}, ID {ExternalId}")]
    public class WalletOperationData
    {
        public string ExternalId { get; set; }
        public string CurrencySymbol { get; set; }
        public double Amount { get; set; }
        public DateTime DateTime { get; set; }
        public string Address { get; set; }
        public string Transaction { get; set; }
    }
}
