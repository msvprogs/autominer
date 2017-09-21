using System;
using System.Collections.Generic;
using System.Linq;

namespace Msv.AutoMiner.Common
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

            return dictionary.TryGetValue(key, out var value)
                ? value
                : defaultValue;
        }

        public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return DoImpl();

            IEnumerable<T> DoImpl()
            {
                foreach (var element in source)
                {
                    action.Invoke(element);
                    yield return element;
                }
            }
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            ForEachImpl();

            void ForEachImpl()
            {
                foreach (var element in source)
                    action.Invoke(element);
            }
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
            => source == null || !source.Any();

        public static T[] EmptyIfNull<T>(this T[] source)
            => source ?? new T[0];

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
            => source ?? Enumerable.Empty<T>();

        public static IEnumerable<TResult> LeftOuterJoin<TLeft, TRight, TKey, TResult>(
            this IEnumerable<TLeft> left,
            IEnumerable<TRight> right,
            Func<TLeft, TKey> leftKeySelector,
            Func<TRight, TKey> rightKeySelector,
            Func<TLeft, TRight, TResult> resultSelector)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));
            if (right == null)
                throw new ArgumentNullException(nameof(right));
            if (leftKeySelector == null)
                throw new ArgumentNullException(nameof(leftKeySelector));
            if (rightKeySelector == null)
                throw new ArgumentNullException(nameof(rightKeySelector));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            return left.GroupJoin(right, leftKeySelector, rightKeySelector,
                (x, y) => resultSelector.Invoke(x, y.FirstOrDefault()));
        }
    }
}
