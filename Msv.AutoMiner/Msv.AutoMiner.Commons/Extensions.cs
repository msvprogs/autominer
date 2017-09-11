using System;
using System.Collections.Generic;

namespace Msv.AutoMiner.Commons
{
    public static class Extensions
    {
        public static TValue TryGetValue<TValue, TKey>(
            this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            TValue value;
            return dictionary.TryGetValue(key, out value)
                ? value
                : defaultValue;
        }

        public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            return DoImpl(source, action);
        }

        private static IEnumerable<T> DoImpl<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var element in source)
            {
                action.Invoke(element);
                yield return element;
            }
        }
    }
}
