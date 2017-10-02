using System;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.FrontEnd.Models.Algorithms
{
    public class AlgorithmModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public KnownCoinAlgorithm? KnownValue { get; set; }
    }
}
