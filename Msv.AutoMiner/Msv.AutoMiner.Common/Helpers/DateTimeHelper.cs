using System;
using System.Globalization;

namespace Msv.AutoMiner.Common.Helpers
{
    public static class DateTimeHelper
    {
        private const string Iso8601Format = "yyyy-MM-dd HH:mm:ss";

        private static readonly DateTime M_EpochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long NowTimestamp => ToTimestamp(DateTime.UtcNow);

        public static long NowTimestampMsec => ToTimestampMsec(DateTime.UtcNow);

        public static long? ToTimestamp(DateTime? dateTime, TimeZoneInfo timeZone = null)
            => dateTime != null ? ToTimestamp(dateTime.Value, timeZone) : (long?)null;

        public static long ToTimestamp(DateTime dateTime, TimeZoneInfo timeZone = null)
            => (long) (TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZone ?? TimeZoneInfo.Utc) - M_EpochStart).TotalSeconds;

        public static long ToTimestampMsec(DateTime dateTime, TimeZoneInfo timeZone = null)
            => (long)(TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZone ?? TimeZoneInfo.Utc) - M_EpochStart).TotalMilliseconds;

        public static DateTime ToDateTimeLocal(long timestamp)
            => TimeZoneInfo.ConvertTimeFromUtc(ToDateTimeUtc(timestamp), TimeZoneInfo.Local);

        public static DateTime ToDateTimeUtc(long timestamp)
            => M_EpochStart.AddSeconds(timestamp);

        public static DateTime ToDateTimeUtcMsec(long timestamp)
            => M_EpochStart.AddMilliseconds(timestamp);

        public static DateTime ToLocalNormalized(DateTime dateTime)
        {
            var local = dateTime.Kind != DateTimeKind.Utc
                ? dateTime
                : TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.Local);
            return Normalize(local);
        }

        public static DateTime Normalize(DateTime dateTime)
            => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);

        public static long TimestampFromIso8601(string dateTimeStr, TimeZoneInfo timeZone = null)
            => ToTimestamp(FromIso8601(dateTimeStr), timeZone);

        /// <summary>
        /// Converts datetime in the ISO 8601 format (yyyy-MM-dd HH:mm:ss) to DateTime value.
        /// </summary>
        /// <param name="dateTimeStr">Datetime string in the ISO 8601 format (yyyy-MM-dd HH:mm:ss)</param>
        /// <returns>Converted DateTime value</returns>
        public static DateTime FromIso8601(string dateTimeStr)
            => FromKnownStringFormat(dateTimeStr, Iso8601Format);

        public static bool TryFromIso8601(string dateTimeStr, out DateTime result)
            => TryFromKnownStringFormat(dateTimeStr.EmptyIfNull(), Iso8601Format, out result);

        public static string ToRelativeTime(DateTime dateTime)
        {
            var utcDateTime = dateTime.Kind == DateTimeKind.Local
                ? TimeZoneInfo.ConvertTimeToUtc(dateTime, TimeZoneInfo.Local)
                : dateTime;
            var difference = DateTime.UtcNow - utcDateTime;

            string postfix;
            if (difference < TimeSpan.Zero)
            {
                postfix = "later";
                difference = difference.Negate();
            }
            else
                postfix = "ago";
            if (difference.TotalSeconds <= 0)
                return "Right now";

            string relativeTimeString;
            if (difference.TotalSeconds < 60)
                relativeTimeString = $"{difference.TotalSeconds:F0} seconds";
            else if (difference.TotalMinutes < 60)
                relativeTimeString = $"{difference.TotalMinutes:F0} minutes";
            else if (difference.TotalHours < 24)
                relativeTimeString = $"{difference.TotalHours:F0} hours";
            else
                relativeTimeString = $"{difference.TotalDays:F0} days";

            return $"{relativeTimeString} {postfix}";
        }

        private static DateTime FromKnownStringFormat(string dateTimeStr, string format)
            => DateTime.ParseExact(dateTimeStr, format, CultureInfo.InvariantCulture);

        private static bool TryFromKnownStringFormat(string dateTimeStr, string format, out DateTime result)
            => DateTime.TryParseExact(dateTimeStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
    }
}
