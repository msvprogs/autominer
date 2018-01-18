using System.Numerics;

namespace Msv.AutoMiner.Common.Helpers
{
    public static class CompactHelper
    {
        private static readonly BigInteger M_MaxCompact = uint.MaxValue;

        public static bool IsCompact(BigInteger value)
            => value >= 0 && value <= M_MaxCompact;

        public static BigInteger FromCompact(uint value)
        {
            var exponent = (int)(value >> 24);
            BigInteger mantissa = value & 0xFFFFFF;
            if (exponent < 3)
                mantissa >>= (3 - exponent) * 8;
            else if (exponent > 3)
                mantissa <<= (exponent - 3) * 8;
            return mantissa;
        }
    }
}