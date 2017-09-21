using System;

namespace Msv.AutoMiner.ControlCenterService.External.Data
{
    public class PoolPaymentData
    {
        public string ExternalId { get; set; }
        public DateTime DateTime { get; set; }
        public double Amount { get; set; }
        public string Transaction { get; set; }
    }
}

