using System;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.Common.Infrastructure
{
    public class ProfitabilityCalculator : IProfitabilityCalculator
    {
        private const int SecondsInDay = 60 * 60 * 24;
        private static readonly double M_32ByteHashesCount = Math.Pow(256, 32);
        private static readonly double M_BtcMaxTarget =
            (double) HexHelper.HexToBigInteger("0x00000000FFFF0000000000000000000000000000000000000000000000000000");

        public double CalculateCoinsPerDay(double difficulty, double blockReward, string maxTarget, double yourHashRate)
        {
            if (difficulty <= 0)
                return 0;
            return SecondsInDay * blockReward * yourHashRate * ParseMaxTarget(maxTarget) / (difficulty * M_32ByteHashesCount);
        }

        public TimeSpan? CalculateTimeToFind(double difficulty, string maxTarget, double hashrate)
        {
            if (hashrate <= 0)
                return null;
            var maxTargetDouble = ParseMaxTarget(maxTarget);
            if (maxTargetDouble <= 0)
                return null;

            return TimeSpan.FromSeconds(difficulty * M_32ByteHashesCount / (maxTargetDouble * hashrate));
        }

        private static double ParseMaxTarget(string maxTarget)
            => string.IsNullOrEmpty(maxTarget)
                ? M_BtcMaxTarget
                : (double)HexHelper.HexToBigInteger(maxTarget);
    }
}
