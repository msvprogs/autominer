using System;
using System.ComponentModel.DataAnnotations;

namespace Msv.AutoMiner.Rig.Storage.Model
{
    public class AlgorithmData
    {
        [Key]
        public Guid AlgorithmId { get; set; }

        [Required]
        public string AlgorithmName { get; set; }

        public long SpeedInHashes { get; set; }

        public double Power { get; set; }

        public int MinerId { get; set; }

        public virtual Miner Miner { get; set; }
    }
}
