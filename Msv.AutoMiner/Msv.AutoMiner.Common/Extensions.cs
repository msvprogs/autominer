using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using JetBrains.Annotations;

namespace Msv.AutoMiner.Common
{
    public static class Extensions
    {
        public static TValue TryGetValue<TValue, TKey>(
            this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
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

        public static void ForEach<T>(this IEnumerable<T> source, [InstantHandle] Action<T> action)
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

        public static string EmptyIfNull(this string str)
            => str ?? string.Empty;

        public static bool Contains(this string str, string substring, StringComparison comparison)
            => str.IndexOf(substring, comparison) >= 0;

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

        public static IEnumerable<TResult> FullOuterJoin<TLeft, TRight, TKey, TResult>(
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

            var leftArray = left.ToArray();
            var rightArray = right.ToArray();
            return leftArray.LeftOuterJoin(rightArray, leftKeySelector, rightKeySelector, resultSelector)
                .Concat(rightArray.GroupJoin(leftArray, rightKeySelector, leftKeySelector,
                        (x, y) => (rightItem: x, hasLeftItem: y.Any()))
                    .Where(x => !x.hasLeftItem)
                    .Select(x => resultSelector.Invoke(default, x.rightItem)));
        }

        public static string Truncate(this string source, int maxLength)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Length > maxLength + 3
                ? source.Substring(0, maxLength) + "..."
                : source;
        }

        public static double ZeroIfNaN(this double source)
            => double.IsNaN(source) ? 0 : source;

        public static double? NullIfNaN(this double? source)
            => source != null && double.IsNaN(source.Value)
                ? null
                : source;

        public static string ToSafeFileName(this string source)
        {
            if (source == null) 
                throw new ArgumentNullException(nameof(source));

            return string.Join("_", source.Split(Path.GetInvalidFileNameChars()));
        }

        public static T ConcatDispose<T>(
            [NotNull] this T disposable, 
            [NotNull] CompositeDisposable composite)
            where T : IDisposable
        {
            if (disposable == null)
                throw new ArgumentNullException(nameof(disposable));
            if (composite == null) 
                throw new ArgumentNullException(nameof(composite));

            composite.Add(disposable);
            return disposable;
        }
    }
}
