using System;
using Microsoft.AspNetCore.Http;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
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
    }
}
