using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Msv.AutoMiner.Commons.Data
{
    [Table("minerspeeds")]
    public class MinerSpeed
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int MinerId { get; set; }

        public DateTime DateTime { get; set; }

        public long SpeedHashesPerSecond { get; set; }

        public virtual Miner Miner { get; set; }
    }
}
