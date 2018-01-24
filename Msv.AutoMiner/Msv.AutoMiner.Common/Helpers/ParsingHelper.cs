using System.Globalization;
using System.Text.RegularExpressions;

namespace Msv.AutoMiner.Common.Helpers
{
    public static class ParsingHelper
    {
        private static readonly CultureInfo M_AmericanCulture = new CultureInfo("en-US");

        private static readonly Regex M_HashRateRegex = new Regex(
            @"(?<value>(\d+[\s,'])*\d+(\.\d+)?)\s*(?<unit>[kmgtp]?[hscd])?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex M_GenerationRewardRegex = new Regex(
            @"Generation:?\s*(?<reward>\d+(\.\d+)?)\s*\+\s*(?<fee>-?\d+(\.\d+)?) total fee", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static double ParseValueWithUnits(string str)
            => ParseDouble(str.Trim().Split()[0]);

        public static long ParseLong(string str)
        {
            var normalizedValue = NormalizeNumber(str);
            return normalizedValue != string.Empty 
                ? long.Parse(normalizedValue, NumberStyles.AllowThousands, M_AmericanCulture) 
                : 0;
        }

        public static double ParseDouble(string str, bool commaIsDecimalPoint = false)
        {
            var normalizedValue = NormalizeNumber(str);
            if (normalizedValue == string.Empty)
                return 0;
            if (commaIsDecimalPoint)
                normalizedValue = normalizedValue.Replace(',', '.');
            return double.Parse(normalizedValue,
                NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign,
                M_AmericanCulture);
        }

        public static double? ParseGenerationReward(string str)
        {
            var match = M_GenerationRewardRegex.Match(str);
            if (!match.Success)
                return null;
            return ParseDouble(match.Groups["reward"].Value)
                   + ParseDouble(match.Groups["fee"].Value);
        }

        public static long ParseHashRate(string str, bool commaIsDecimalPoint = false)
        {
            if (string.IsNullOrWhiteSpace(str))
                return 0;
            var match = M_HashRateRegex.Match(str);
            if (!match.Success)
                return 0;
            var value = match.Groups["value"].Value;
            var unit = match.Groups["unit"].Value.ToLowerInvariant();
            if (unit == string.Empty)
                return (long) ParseDouble(value, commaIsDecimalPoint);
            double multiplier;
            if (unit.StartsWith("h") || unit.StartsWith("s"))
                multiplier = 1;
            else if (unit.StartsWith("k"))
                multiplier = 1e3;
            else if (unit.StartsWith("m"))
                multiplier = 1e6;
            else if (unit.StartsWith("g"))
                multiplier = 1e9;
            else if (unit.StartsWith("t"))
                multiplier = 1e12;
            else if (unit.StartsWith("p"))
                multiplier = 1e15;
            else
                multiplier = 1;
            return (long) (ParseDouble(value) * multiplier);
        }

        private static string NormalizeNumber(string str)
            => str?.Replace("#", string.Empty)
                .Replace(" ", string.Empty)
                .Replace("'", string.Empty)
                .Replace("%", string.Empty)
                .Trim() ?? string.Empty;
    }
}
