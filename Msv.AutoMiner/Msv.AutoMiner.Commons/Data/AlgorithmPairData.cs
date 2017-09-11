using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Msv.AutoMiner.Commons.Data
{
    [Table("algorithmpairdatas")]
    public class AlgorithmPairData
    {
        [Key]
        [Column(Order = 0)]
        public CoinAlgorithm Algorithm1 { get; set; }

        [Key]
        [Column(Order = 1)]
        public CoinAlgorithm Algorithm2 { get; set; }

        public long SpeedInHashes1 { get; set; }

        public long SpeedInHashes2 { get; set; }

        public int MinerId { get; set; }

        public bool IsActive { get; set; }

        public double Power { get; set; }

        public virtual Miner Miner { get; set; }
    }
}
