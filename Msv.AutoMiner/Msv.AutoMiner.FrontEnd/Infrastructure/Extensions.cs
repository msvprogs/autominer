using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    [Obfuscation(Exclude = true)]
    public static class Extensions
    {
        public static double? GetDouble(this ISession session, string key)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            if (!session.TryGetValue(key, out var bytes))
                return null;
            return BitConverter.ToDouble(bytes, 0);
        }

        public static void SetDouble(this ISession session, string key, double value)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            session.Set(key, BitConverter.GetBytes(value));
        }

        public static bool? GetBool(this ISession session, string key)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            if (!session.TryGetValue(key, out var bytes))
                return null;
            return BitConverter.ToBoolean(bytes, 0);
        }

        public static void SetBool(this ISession session, string key, bool value)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            session.Set(key, BitConverter.GetBytes(value));
        }

        public static T GetEnum<T>(this ISession session, string key)
            where T : struct
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            if (!session.TryGetValue(key, out var bytes))
                return default;
            return (T) Convert.ChangeType(BitConverter.ToUInt64(bytes, 0), Enum.GetUnderlyingType(typeof(T)));
        }

        public static void SetEnum<T>(this ISession session, string key, T value)
            where T : struct
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            session.Set(key, BitConverter.GetBytes(Convert.ToUInt64(value)));
        }

        public static void AddClasses(this TagHelperAttributeList attributes, params string[] newClasses)
        {
            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));

            const string classAttributeKey = "class";
            attributes.SetAttribute(classAttributeKey,
                attributes.TryGetAttribute(classAttributeKey, out var oldClass)
                    ? string.Join(" ", new[] {oldClass.Value}.Concat(newClasses))
                    : string.Join(" ", newClasses));
        }
    }
}
