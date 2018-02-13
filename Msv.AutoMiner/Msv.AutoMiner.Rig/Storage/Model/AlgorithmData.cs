using System.ComponentModel.DataAnnotations;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.Rig.Storage.Model
{
    public class AlgorithmData
    {
        [Key, MaxLength(64)]
        public string AlgorithmId { get; set; }

        [Required, MaxLength(64)]
        public string AlgorithmName { get; set; }

        public KnownCoinAlgorithm? KnownValue { get; set; }

        public long SpeedInHashes { get; set; }

        public double Power { get; set; }
    }
}
