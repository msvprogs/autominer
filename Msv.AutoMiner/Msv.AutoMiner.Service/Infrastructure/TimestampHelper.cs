using System;

namespace Msv.AutoMiner.Service.Infrastructure
{
    public static class TimestampHelper
    {
        private static readonly DateTime M_EpochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long Now => ToTimestamp(DateTime.Now, TimeZoneInfo.Local);

        public static long ToTimestamp(DateTime dateTime, TimeZoneInfo timeZone)
            => (long) (TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZone) - M_EpochStart).TotalSeconds;

        public static DateTime ToDateTime(long timestamp)
            => TimeZone.CurrentTimeZone.ToLocalTime(M_EpochStart.AddSeconds(timestamp));

        public static DateTime ToLocalNormalized(DateTime dateTime)
        {
            var local = TimeZone.CurrentTimeZone.ToLocalTime(dateTime);
            return new DateTime(local.Year, local.Month, local.Day, local.Hour, local.Minute, local.Second);
        }
    }
}
