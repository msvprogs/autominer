using System;

namespace Msv.AutoMiner.Common.Helpers
{
    public static class TimeSpanHelper
    {
        public static string ToShortString(TimeSpan? timeSpan)
        {
            if (timeSpan == null)
                return "N/A";

            if (timeSpan.Value.TotalSeconds < 60)
                return $"{timeSpan.Value.TotalSeconds:F1} s";
            if (timeSpan.Value.TotalMinutes < 60)
                return $"{timeSpan.Value.TotalMinutes:F1} m";
            if (timeSpan.Value.TotalHours < 24)
                return $"{timeSpan.Value.TotalHours:F1} h";
            return $"{timeSpan.Value.TotalDays:F1} d";
        }
    }
}
