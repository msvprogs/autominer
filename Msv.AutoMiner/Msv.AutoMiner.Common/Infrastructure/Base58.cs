using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Msv.AutoMiner.Common.Infrastructure
{
    public static class Base58
    {        
        private const string Symbols = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        private const string ZeroSymbol = "1";
        private const int Base = 58;

        private static readonly Dictionary<char, int> M_SymbolIndexes = Symbols
            .Select((x, i) => (symbol:x, index:i))
            .ToDictionary(x => x.symbol, x => x.index);

        private static readonly Regex M_CheckRegex = new Regex($"[^{Symbols}]", RegexOptions.Compiled);

        public static bool IsValidString(string value)
            => !M_CheckRegex.IsMatch(value);

        public static byte[] Decode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new byte[0];

            var resultBytes = value
                .Aggregate(BigInteger.Zero, (x, y) => x * Base + M_SymbolIndexes[y])
                .ToByteArray();
            if (value.StartsWith(ZeroSymbol))
            {
                var zeroBytes = value.TakeWhile(x => x == ZeroSymbol[0]).Count();
                var existingZeroBytes = resultBytes.Reverse().TakeWhile(x => x == 0).Count();
                if (zeroBytes > existingZeroBytes)
                    Array.Resize(ref resultBytes, resultBytes.Length + zeroBytes - existingZeroBytes);
                Array.Reverse(resultBytes);
            }
            else if (resultBytes.Last() == 0)
                resultBytes = resultBytes.Reverse().SkipWhile(x => x == 0).ToArray();
            else
                Array.Reverse(resultBytes);

            return resultBytes;
        }

        public static string Encode(byte[] bytes)
        {
            if (bytes.IsNullOrEmpty())
                return string.Empty;

            var sourceInteger = new BigInteger(
                new byte[] {0}.Concat(bytes).Reverse().ToArray());
            var resultSymbols = new List<char>(bytes.Length);
            while (sourceInteger > 0)
            {
                sourceInteger = BigInteger.DivRem(sourceInteger, Base, out var remainder);
                resultSymbols.Add(Symbols[(int) remainder]);
            }

            var leadingZeroes = bytes.TakeWhile(x => x == 0).Count();
            if (leadingZeroes > 0)
                resultSymbols.AddRange(Enumerable.Repeat(ZeroSymbol[0], leadingZeroes));

            resultSymbols.Reverse();
            return new string(resultSymbols.ToArray());
        }
    }
}
