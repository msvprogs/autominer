using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Msv.AutoMiner.Common.Infrastructure;

namespace Msv.AutoMiner.Common.Helpers
{
    public static class EnumHelper
    {
        public static IEnumerable<T> GetValues<T>()
            where T : struct
        {
            ValidateEnumType<T>();

            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static IDictionary<T, string> GetCaptions<T>()
            where T : struct
        {
            ValidateEnumType<T>();

            return typeof(T)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .ToDictionary(
                    x => (T) x.GetValue(null),
                    x => ((EnumCaptionAttribute) x.GetCustomAttribute(typeof(EnumCaptionAttribute)))?.Caption ?? x.Name);
        }

        public static T Parse<T>(string value, bool ignoreCase = false)
            where T : struct
        {
            ValidateEnumType<T>();

            return (T) Enum.Parse(typeof(T), value, ignoreCase);
        }

        private static void ValidateEnumType<T>()
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentOutOfRangeException(nameof(T), "Not an enum type");
        }
    }
}
