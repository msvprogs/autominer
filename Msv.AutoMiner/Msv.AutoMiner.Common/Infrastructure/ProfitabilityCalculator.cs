using System;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.Common.Infrastructure
{
    public class ProfitabilityCalculator : IProfitabilityCalculator
    {
        private const int SecondsInDay = 60 * 60 * 24;

        // Assuming that probability of finding the longer chain for PrimeChain is 0.0291 (from statistical data)
        // How to calculate: (s1+s2)/(smin+s1+s2), where smin - shares of min length, s1 - shares of min length + 1 and so on.
        // Numbers of shares of each length are available at http://xpmforall.org/ (24h Share count)
        private const double LongerChainProbability = 0.0291;

        private static readonly double M_32ByteHashesCount = Math.Pow(256, 32);
        private static readonly double M_BtcMaxTarget = (double) CompactHelper.FromCompact(0x1d00ffff);

        public double CalculateCoinsPerDay(
            KnownCoinAlgorithm? knownAlgorithm, double difficulty, double blockReward, string maxTarget,
            double yourHashRate)
        {
            if (difficulty <= 0)
                return 0;
            // PrimeChain difficulty is like 10.8888, where 10 is a minimal chain length 
            // and (1 - 0.8888) is a probability that the found chain of length 10 satisfies mining target conditions.
            // We assume that the output of failed Ferma probability test of the 11-th element is uniformely distributed.
            // Longer chains will be accepted with 100% probability.
            if (knownAlgorithm == KnownCoinAlgorithm.PrimeChain)
                return blockReward * yourHashRate * CalculatePrimeChainFindingProbability(difficulty);
            return SecondsInDay * blockReward * yourHashRate * ParseMaxTarget(maxTarget) /
                   (difficulty * M_32ByteHashesCount);
        }

        public TimeSpan? CalculateTimeToFind(
            KnownCoinAlgorithm? knownAlgorithm, double difficulty, string maxTarget, double hashrate)
        {
            if (hashrate <= 0)
                return null;

            // PrimeChain hashrate time units is Days (Chains Per Day)
            if (knownAlgorithm == KnownCoinAlgorithm.PrimeChain)
                return TimeSpan.FromDays(1 / (hashrate * CalculatePrimeChainFindingProbability(difficulty)));

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


        // Probability = current_length_probability + longer_length_probability
        private static double CalculatePrimeChainFindingProbability(double difficulty)
            => (1 - (difficulty - Math.Truncate(difficulty))) * (1 - LongerChainProbability) + LongerChainProbability;

        private static double ParseMaxTarget(string maxTarget)
        {
            if (string.IsNullOrEmpty(maxTarget))
                return M_BtcMaxTarget;
            var parsedTarget = HexHelper.HexToBigInteger(maxTarget);
            return CompactHelper.IsCompact(parsedTarget)
                ? (double) CompactHelper.FromCompact((uint) parsedTarget)
                : (double) parsedTarget;
        }
    }
}
