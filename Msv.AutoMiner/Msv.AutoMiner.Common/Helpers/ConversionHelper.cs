using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.Common.Helpers
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

        private static readonly Dictionary<KnownCoinAlgorithm, string> M_SpecialUnits =
            new Dictionary<KnownCoinAlgorithm, string>
            {
                [KnownCoinAlgorithm.Equihash] = "Sol/s",
                [KnownCoinAlgorithm.Mars] = "Sol/s",
                [KnownCoinAlgorithm.PrimeChain] = "CPD"
            };

        public static string ToHashRateWithUnits(long hashRate, KnownCoinAlgorithm? algorithm = KnownCoinAlgorithm.Unknown)
            => ToHashRateWithUnits((double) hashRate, algorithm);

        public static string ToHashRateWithUnits(double hashRate, KnownCoinAlgorithm? algorithm = KnownCoinAlgorithm.Unknown)
        {
            var prefixKeyPair = M_Prefixes
                .OrderByDescending(x => x.Key)
                .FirstOrDefault(x => hashRate >= x.Key);
            var normalizedHashRate = prefixKeyPair.Key > 0
                ? hashRate / prefixKeyPair.Key
                : hashRate;
            var unit = M_SpecialUnits.TryGetValue(algorithm.GetValueOrDefault(KnownCoinAlgorithm.Unknown), "H/s");
            return $"{normalizedHashRate:F3} {prefixKeyPair.Value}{unit}";
        }

        public static string ToCryptoCurrencyValue(double value)
            => value.ToString("N8");

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
