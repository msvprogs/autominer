using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Msv.AutoMiner.Commons.Data
{
    [Table("algorithmdatas")]
    public class AlgorithmData
    {
        [Key]
        public CoinAlgorithm Algorithm { get; set; }
        public long SpeedInHashes { get; set; }
        public long Multiplier { get; set; }
        public double Power { get; set; }
    }
}
