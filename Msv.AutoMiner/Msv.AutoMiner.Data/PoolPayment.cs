using System;

namespace Msv.AutoMiner.Data
{
    public class PoolPayment
    {
        public int PoolId { get; set; }

        public virtual Pool Pool { get; set; }

        public string ExternalId { get; set; }

        public DateTime DateTime { get; set; }

        public double Amount { get; set; }

        public string Transaction { get; set; }
    }
}
