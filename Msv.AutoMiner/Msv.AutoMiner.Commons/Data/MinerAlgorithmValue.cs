using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Msv.AutoMiner.Commons.Data
{
    [Table("mineralgorithmvalues")]
    public class MinerAlgorithmValue
    {
        [Key]
        [Column(Order = 0)]
        public CoinAlgorithm Algorithm { get; set; }

        [Key]
        [Column(Order = 1)]
        public int MinerId { get; set; }

        [MaxLength(256)]
        public string Value { get; set; }
    }
}
