using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace Msv.AutoMiner.Common.Helpers
{
    public static class HexHelper
    {
        private static readonly Regex M_HexRegex = new Regex(
            @"^(0x)?[0-9a-f]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string ToHex(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            var builder = new StringBuilder();
            foreach (var @byte in bytes)
                builder.AppendFormat("{0:x2}", @byte);
            return builder.ToString();
        }

        public static byte[] FromHex(string str)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));
            if (string.IsNullOrWhiteSpace(str))
                return new byte[0];

            str = str.Trim();
            if (str.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
                str = str.Substring(2);
            str = str.Replace("-", "").Replace(" ", "");
            if (str.Length % 2 != 0)
                str = "0" + str;
            var bytes = new byte[str.Length / 2];
            for (var i = 0; i < str.Length; i += 2)
                bytes[i / 2] = byte.Parse(str.Substring(i, 2), NumberStyles.HexNumber);
            return bytes;
        }

        public static BigInteger HexToBigInteger(string str)
        {
            var bytes = FromHex(str);
            if (bytes.Length == 0)
                return BigInteger.Zero;
            Array.Reverse(bytes);
            if ((bytes.Last() & 0x80) != 0)
                Array.Resize(ref bytes, bytes.Length + 1);
            return new BigInteger(bytes);
        }

        public static bool IsHex(string str)
            => M_HexRegex.IsMatch(str.Trim());
    }
}
