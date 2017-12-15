using System;

namespace Msv.AutoMiner.Common.Infrastructure
{
    public interface IProfitabilityCalculator
    {
        double CalculateCoinsPerDay(double difficulty, double blockReward, string maxTarget, double yourHashRate);
        TimeSpan? CalculateTimeToFind(double difficulty, string maxTarget, double hashrate);
    }
}
