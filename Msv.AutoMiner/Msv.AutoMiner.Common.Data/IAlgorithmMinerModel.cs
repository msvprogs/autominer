using System;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.Common.Data
{
    public interface IAlgorithmMinerModel
    {
        Guid AlgorithmId { get; }

        string AlgorithmName { get; }

        KnownCoinAlgorithm? KnownValue { get; }

        int MinerId { get; }

        string AlgorithmArgument { get; }

        double? Intensity { get; }

        string AdditionalArguments { get; }
    }
}