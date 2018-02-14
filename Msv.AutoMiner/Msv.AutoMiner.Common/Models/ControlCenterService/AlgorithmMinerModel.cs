using System;
using Msv.AutoMiner.Common.Data;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.Common.Models.ControlCenterService
{
    public class AlgorithmMinerModel : IAlgorithmMinerModel
    {
        public Guid AlgorithmId { get; set; }

        public string AlgorithmName { get; set; }

        public KnownCoinAlgorithm? KnownValue { get; set; }

        public int MinerId { get;set; }

        public string AlgorithmArgument { get; set; }

        public double? Intensity { get; set; }

        public string AdditionalArguments { get; set; }
    }
}
