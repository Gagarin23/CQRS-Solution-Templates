using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Extensions
{
    public static class QueryableExtensions
    {
        public static async Task<Dictionary<TKey, TElement>> ToDictionarySafelyAsync<TSource, TKey, TElement>
        (
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector,
            [NotNull] Func<TSource, TElement> elementSelector,
            int capacity = 0,
            CancellationToken cancellationToken = default
        )
        {
            var d = new Dictionary<TKey, TElement>(capacity);
            await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                d.TryAdd(keySelector(element), elementSelector(element));
            }

            return d;
        }

        public static async Task<Dictionary<TKey, List<TElement>>> ToDictionarySafelyWithGroupingAsync<TSource, TKey, TElement>
        (
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector,
            [NotNull] Func<TSource, TElement> elementSelector,
            int capacity = 0,
            CancellationToken cancellationToken = default
        )
        {
            var d = new Dictionary<TKey, List<TElement>>(capacity);
            await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                var key = keySelector(element);
                var value = elementSelector(element);

                if (d.TryGetValue(key, out var existValue))
                {
                    existValue.Add(elementSelector(element));
                }
                else
                {
                    d.Add(key, new List<TElement>(){ value });
                }
            }

            return d;
        }

        public static async Task<Dictionary<TKey, TElement>> ToDictionarySafelyAsync<TSource, TKey, TElement>
        (
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector,
            [NotNull] Func<TSource, TElement> elementSelector,
            [NotNull] Func<TElement, TElement, TElement> mergeFunction,
            int capacity = 0,
            CancellationToken cancellationToken = default
        )
        {
            var d = new Dictionary<TKey, TElement>(capacity);
            await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                var key = keySelector(element);
                var value = elementSelector(element);
                
                if (d.TryGetValue(key, out var existValue))
                {
                    var correctValue = mergeFunction(existValue, value);
                    d[key] = correctValue;
                }
                else
                {
                    d.Add(key, value); 
                }
            }

            return d;
        }

        public static async Task<HashSet<TSource>> ToHashSetAsync<TSource>([NotNull] this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            var set = new HashSet<TSource>();

            await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                set.Add(element);
            }

            return set;
        }
    }
}
