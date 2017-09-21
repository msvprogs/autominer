using System;

namespace Msv.AutoMiner.Data
{
    public class PoolAccountState
    {
        public int PoolId { get; set; }

        public virtual Pool Pool { get; set; }

        public DateTime DateTime { get; set; }

        public double ConfirmedBalance { get; set; }

        public double UnconfirmedBalance { get; set; }

        public long HashRate { get; set; }

        public int ValidShares { get; set; }

        public int InvalidShares { get; set; }

        public long PoolHashRate { get; set; }

        public int PoolWorkers { get; set; }

        public long? PoolLastBlock { get; set; }
    }
}
