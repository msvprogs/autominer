using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Msv.AutoMiner.Common.Helpers
{
    public static class ParsingHelper
    {
        private const NumberStyles DoubleNumberStyles =
            NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign;

        private static readonly CultureInfo M_AmericanCulture = new CultureInfo("en-US");

        private static readonly Regex M_HashRateRegex = new Regex(
            @"(?<value>(\d+[\s,'])*\d+(\.\d+)?)\s*(?<unit>[kmgtpe]?[hscd])?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Dictionary<char, double> M_PowersOf10 =
            new Dictionary<char, double>
            {
                ['k'] = 1e3,
                ['m'] = 1e6,
                ['g'] = 1e9,
                ['t'] = 1e12,
                ['p'] = 1e15,
                ['e'] = 1e18
            };

        public static double ParseValueWithUnits(string str)
            => ParseDouble(str.Trim().Split()[0]);

        public static bool TryParseValueWithUnits(string str, out double result)
            => TryParseDouble(str?.Trim().Split()[0], out result);

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
            return double.Parse(normalizedValue, DoubleNumberStyles, M_AmericanCulture);
        }

        public static bool TryParseDouble(string str, out double result)
        {
            var normalizedValue = NormalizeNumber(str);
            if (normalizedValue == string.Empty)
            {
                result = 0;
                return true;
            }
            return double.TryParse(normalizedValue, DoubleNumberStyles, M_AmericanCulture, out result);
        }

        public static double ParseHashRate(string str, bool commaIsDecimalPoint = false)
        {
            if (string.IsNullOrWhiteSpace(str))
                return 0;
            var match = M_HashRateRegex.Match(str);
            if (!match.Success)
                return 0;
            var value = match.Groups["value"].Value;
            var unit = match.Groups["unit"].Value.ToLowerInvariant();
            if (unit == string.Empty)
                return ParseDouble(value, commaIsDecimalPoint);
            return ParseDouble(value) * M_PowersOf10.TryGetValue(unit[0], 1);
        }

        private static string NormalizeNumber(string str)
            => str?.Replace("#", string.Empty)
                .Replace(" ", string.Empty)
                .Replace("'", string.Empty)
                .Replace("%", string.Empty)
                .Trim() ?? string.Empty;
    }
}
