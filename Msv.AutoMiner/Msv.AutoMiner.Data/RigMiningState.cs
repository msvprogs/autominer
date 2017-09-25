using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Msv.AutoMiner.Data
{
    public class RigMiningState
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public int RigId { get; set; }

        public DateTime DateTime { get; set; }

        public Guid CoinId { get; set; }

        public int ValidShares { get; set; }

        public int InvalidShares { get; set; }

        public long HashRate { get; set; }
    }
}
