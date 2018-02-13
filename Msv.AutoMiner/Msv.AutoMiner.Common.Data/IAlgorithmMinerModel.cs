using System;

namespace Msv.AutoMiner.Common.Data
{
    public interface IAlgorithmMinerModel
    {
        Guid AlgorithmId { get; }

        string AlgorithmName { get; }

        int MinerId { get; }

        string AlgorithmArgument { get; }

        double? Intensity { get; }

        string AdditionalArguments { get; }
    }
}