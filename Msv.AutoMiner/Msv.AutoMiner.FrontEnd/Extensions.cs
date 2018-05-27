using System;
using System.Globalization;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.FrontEnd
{
    public static class Extensions
    {
        public static string ToDataOrder(this double source)
            => source.ToString(CultureInfo.InvariantCulture);

        public static string ToDataOrderBtc(this double source, double btcRate)
            => (source * btcRate).ToString(CultureInfo.InvariantCulture);
        
        public static string ToDataOrder(this double? source)
            => source?.ToString(CultureInfo.InvariantCulture);

        public static long ToDataOrder(this DateTime source)
            => DateTimeHelper.ToTimestamp(source);
        
        public static long? ToDataOrder(this DateTime? source)
            => DateTimeHelper.ToTimestamp(source);

        public static string ToDataOrder(this TimeSpan source)
            => ToDataOrder(source.TotalSeconds);

        public static string ToDataOrder(this TimeSpan? source)
            => ToDataOrder(source?.TotalSeconds);
    }
}
