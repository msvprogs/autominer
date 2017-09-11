using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Msv.AutoMiner.Commons.Data
{
    [Table("poolaccountstates")]
    public class PoolAccountState
    {
        [Key]
        [Column(Order = 0)]
        public int PoolId { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime DateTime { get; set; }

        public double ConfirmedBalance { get; set; }

        public double UnconfirmedBalance { get; set; }

        public long HashRate { get; set; }

        public int ValidShares { get; set; }

        public int InvalidShares { get; set; }

        public long PoolHashRate { get; set; }

        public int PoolWorkers { get; set; }

        public long? PoolLastBlock { get; set; }

        public virtual Pool Pool { get; set; }
    }
}
