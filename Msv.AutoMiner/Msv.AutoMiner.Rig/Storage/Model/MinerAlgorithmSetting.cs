using System;

namespace Msv.AutoMiner.Rig.Storage.Model
{
    public class MinerAlgorithmSetting
    {
        public Guid AlgorithmId { get; set; }

        public int MinerId { get; set; }

        public virtual Miner Miner { get; set; }

        public string AlgorithmArgument { get; set; }

        public double? Intensity { get; set; }

        public string LogFile { get; set; }

        public string AdditionalArguments { get; set; }
    }
}
