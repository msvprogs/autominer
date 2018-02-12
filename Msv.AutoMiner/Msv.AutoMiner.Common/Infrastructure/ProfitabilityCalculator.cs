using System;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.Common.Infrastructure
{
    public class ProfitabilityCalculator : IProfitabilityCalculator
    {
        private const int SecondsInDay = 60 * 60 * 24;
        private static readonly double M_32ByteHashesCount = Math.Pow(256, 32);
        private static readonly double M_BtcMaxTarget = (double) CompactHelper.FromCompact(0x1d00ffff);

        public double CalculateCoinsPerDay(
            KnownCoinAlgorithm? knownAlgorithm, double difficulty, double blockReward, string maxTarget, double yourHashRate)
        {
            if (difficulty <= 0)
                return 0;
            // PrimeChain difficulty is like 10.8888, where 10 is a minimal chain length 
            // and (1 - 0.8888) is a probability that the found chain of length 10 satisfies mining target conditions.
            // We assume that the output of failed Ferma probability test of the 11-th element is uniformely distributed.
            // Longer chains will be accepted with 100% probability.
            if (knownAlgorithm == KnownCoinAlgorithm.PrimeChain)
                return blockReward * yourHashRate * (1 - (difficulty - Math.Truncate(difficulty)));
            return SecondsInDay * blockReward * yourHashRate * ParseMaxTarget(maxTarget) / (difficulty * M_32ByteHashesCount);
        }

        public TimeSpan? CalculateTimeToFind(
            KnownCoinAlgorithm? knownAlgorithm, double difficulty, string maxTarget, double hashrate)
        {
            if (hashrate <= 0)
                return null;

            // PrimeChain hashrate time units is Days (Chains Per Day)
            if (knownAlgorithm == KnownCoinAlgorithm.PrimeChain)
                return TimeSpan.FromDays(1 / (hashrate * (1 - (difficulty - Math.Truncate(difficulty)))));

            var maxTargetDouble = ParseMaxTarget(maxTarget);
            if (maxTargetDouble <= 0)
                return null;

            var ttfSeconds = difficulty * M_32ByteHashesCount / (maxTargetDouble * hashrate);
            if (double.IsNaN(ttfSeconds) 
                || double.IsInfinity(ttfSeconds)
                || ttfSeconds > TimeSpan.MaxValue.TotalSeconds)
                return null;
            return TimeSpan.FromSeconds(ttfSeconds);
        }

        private static double ParseMaxTarget(string maxTarget)
        {
            if (string.IsNullOrEmpty(maxTarget))
                return M_BtcMaxTarget;
            var parsedTarget = HexHelper.HexToBigInteger(maxTarget);
            return CompactHelper.IsCompact(parsedTarget)
                ? (double)CompactHelper.FromCompact((uint) parsedTarget)
                : (double)parsedTarget;
        }
    }
}
