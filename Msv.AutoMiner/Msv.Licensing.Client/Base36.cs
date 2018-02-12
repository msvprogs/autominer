using System;
using System.Linq;
using System.Reflection;

namespace Msv.Licensing.Client
{
    public static class Base36
    {
        private const string GroupDelimiter = "-";
        private const string Symbols = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const uint Base = 36;

        public static string EncodeUint(uint value)
        {
            var chars = new char[7];
            var i = chars.Length;
            do
            {
                chars[--i] = Symbols[(int) (value % Base)];
                value /= Base;
            } while (value > 0);
            return new string(chars, i, chars.Length - i);
        }

        [Obfuscation(Exclude = true)]
        public static string EncodeDelimited(byte[] bytes, int groups, int groupLength = 5) 
            => string.Join(GroupDelimiter, Enumerable.Range(0, groups)
                .Select(x => x * sizeof(uint))
                .Select(x => BitConverter.ToUInt32(bytes, x))
                .Select(EncodeUint)
                .Select(x =>
                {
                    var part = x.PadLeft(groupLength, Symbols[0]);
                    return part.Substring(part.Length - groupLength);
                }));
    }
}
