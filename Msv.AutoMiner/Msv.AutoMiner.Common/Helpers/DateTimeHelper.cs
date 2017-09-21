﻿using System;
using System.Globalization;

namespace Msv.AutoMiner.Common.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly DateTime M_EpochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long Now => ToTimestamp(DateTime.UtcNow, TimeZoneInfo.Utc);

        public static long ToTimestamp(DateTime dateTime, TimeZoneInfo timeZone)
            => (long) (TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZone) - M_EpochStart).TotalSeconds;

        public static DateTime ToDateTimeLocal(long timestamp)
            => TimeZoneInfo.ConvertTimeFromUtc(ToDateTimeUtc(timestamp), TimeZoneInfo.Local);

        public static DateTime ToDateTimeUtc(long timestamp)
            => M_EpochStart.AddSeconds(timestamp);

        public static DateTime ToLocalNormalized(DateTime dateTime)
        {
            var local = dateTime.Kind != DateTimeKind.Utc
                ? dateTime
                : TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.Local);
            return Normalize(local);
        }

        public static DateTime Normalize(DateTime dateTime)
            => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);

        public static long TimestampFromIso8601DateTime(string dateTimeStr, TimeZoneInfo timeZone = null)
            => TimestampFromKnownStringFormat(dateTimeStr, "yyyy-MM-dd HH:mm:ss", timeZone);

        public static long TimestampFromRussianDateTime(string dateTimeStr, TimeZoneInfo timeZone = null)
            => TimestampFromKnownStringFormat(dateTimeStr, "dd.MM.yyyy HH:mm:ss", timeZone);

        private static long TimestampFromKnownStringFormat(string dateTimeStr, string format, TimeZoneInfo timeZone = null)
            => ToTimestamp(DateTime.ParseExact(dateTimeStr, format, CultureInfo.InvariantCulture),
                timeZone ?? TimeZoneInfo.Utc);
    }
}
