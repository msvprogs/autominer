using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.Common.Helpers
{
    public static class ConversionHelper
    {
        private const string SolS = "Sol/s";

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
                [KnownCoinAlgorithm.Equihash] = SolS,
                [KnownCoinAlgorithm.Mars] = SolS,
                [KnownCoinAlgorithm.Equihash1927] = SolS,
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

        public static string ToCryptoCurrencyValue(double? value)
            => value != null ? value.Value.ToString("N8") : string.Empty;

        public static string ToFiatValue(double? value)
            => value != null ? value.Value.ToString("N2") : string.Empty;

        public static string ToPercent(double? value)
            => value != null ? value.Value.ToString("F2") + "%" : string.Empty;

        public static double GetDiffRatio(decimal oldValue, decimal newValue)
            => GetDiffRatio((double) oldValue, (double) newValue);

        public static double GetDiffRatio(double oldValue, double newValue) 
            => Math.Abs(oldValue) > double.Epsilon
                ? (newValue - oldValue) / oldValue * 100
                : 0;

        public static string GetDiffRatioString(double oldValue, double newValue)
            => ToPercent(GetDiffRatio(oldValue, newValue));
    }
}
