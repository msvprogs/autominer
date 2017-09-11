using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Commons;
using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.Infrastructure
{
    public static class ConversionHelper
    {
        private static readonly Dictionary<double, string> M_Prefixes =
            new Dictionary<double, string>
            {
                [1e18] = "E",
                [1e15] = "P",
                [1e12] = "T",
                [1e9] = "G",
                [1e6] = "M",
                [1e3] = "k",
                [1] = string.Empty
            };

        private static readonly Dictionary<CoinAlgorithm, string> M_SpecialUnits =
            new Dictionary<CoinAlgorithm, string>
            {
                [CoinAlgorithm.Equihash] = "Sol/s",
                [CoinAlgorithm.PrimeChain] = "CPD"
            };

        public static string ToHashRateWithUnits(long hashRate, CoinAlgorithm algorithm)
        {
            if (hashRate < 0)
                throw new ArgumentOutOfRangeException(nameof(hashRate));

            var prefixKeyPair = M_Prefixes
                .OrderByDescending(x => x.Key)
                .FirstOrDefault(x => hashRate >= x.Key);
            var normalizedHashRate = prefixKeyPair.Key > 0
                ? hashRate / prefixKeyPair.Key
                : hashRate;
            var unit = M_SpecialUnits.TryGetValue(algorithm, "H/s");
            return $"{normalizedHashRate:F3} {prefixKeyPair.Value}{unit}";
        }

        public static double GetDiffRatio(decimal oldValue, decimal newValue)
            => GetDiffRatio((double) oldValue, (double) newValue);

        public static double GetDiffRatio(double oldValue, double newValue)
            => oldValue < newValue
                ? (newValue / oldValue - 1) * 100
                : -(oldValue / newValue - 1) * 100;

        public static string GetDiffRatioString(double oldValue, double newValue)
        {
            var result = GetDiffRatio(oldValue, newValue);
            if (double.IsNaN(result) || double.IsInfinity(result))
                return "<unknown>";
            return $"{result:F2}%";
        }
    }
}
