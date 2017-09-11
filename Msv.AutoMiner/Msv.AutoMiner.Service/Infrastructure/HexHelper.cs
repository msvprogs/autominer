using System;
using System.Globalization;
using System.Text;

namespace Msv.AutoMiner.Service.Infrastructure
{
    public static class HexHelper
    {
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

            if (str.StartsWith("0x"))
                str = str.Substring(2);
            if (str.Length % 2 != 0)
                str = "0" + str;
            var bytes = new byte[str.Length / 2];
            for (var i = 0; i < str.Length; i += 2)
                bytes[i / 2] = byte.Parse(str.Substring(i, 2), NumberStyles.HexNumber);
            return bytes;
        }
    }
}
