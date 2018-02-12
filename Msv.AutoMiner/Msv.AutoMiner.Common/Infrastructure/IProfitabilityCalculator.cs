using System;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.Common.Infrastructure
{
    public interface IProfitabilityCalculator
    {
        double CalculateCoinsPerDay(
            KnownCoinAlgorithm? knownAlgorithm, double difficulty, double blockReward, string maxTarget, double yourHashRate);
        TimeSpan? CalculateTimeToFind(KnownCoinAlgorithm? knownAlgorithm, double difficulty, string maxTarget, double hashrate);
    }
}
