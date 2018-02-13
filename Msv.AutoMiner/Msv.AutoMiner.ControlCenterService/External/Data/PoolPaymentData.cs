using System;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.ControlCenterService.External.Data
{
    public class PoolPaymentData
    {
        public string ExternalId { get; set; }
        public DateTime DateTime { get; set; }
        public double Amount { get; set; }
        public string Transaction { get; set; }
        public string BlockHash { get; set; }
        public PoolPaymentType Type { get; set; }
        public string Address { get; set; }
    }
}

