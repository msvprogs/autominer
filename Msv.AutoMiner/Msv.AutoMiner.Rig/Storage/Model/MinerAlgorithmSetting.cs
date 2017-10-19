using System.ComponentModel.DataAnnotations;

namespace Msv.AutoMiner.Rig.Storage.Model
{
    public class MinerAlgorithmSetting
    {
        [MaxLength(64)]
        public string AlgorithmId { get; set; }

        public virtual AlgorithmData Algorithm { get; set; }

        public int MinerId { get; set; }

        public virtual Miner Miner { get; set; }

        public string AlgorithmArgument { get; set; }

        public double? Intensity { get; set; }

        public string LogFile { get; set; }

        public string AdditionalArguments { get; set; }
    }
}
