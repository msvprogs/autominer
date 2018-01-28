using System;

namespace Msv.AutoMiner.Common.Models.ControlCenterService
{
    public class AlgorithmMinerModel : IAlgorithmMinerModel
    {
        public Guid AlgorithmId { get; set; }

        public string AlgorithmName { get; set; }

        public int MinerId { get;set; }

        public string AlgorithmArgument { get; set; }

        public double? Intensity { get; set; }

        public string AdditionalArguments { get; set; }
    }
}
