using Fed.Core.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> source) => source ?? Enumerable.Empty<T>();

        public static int RemoveAll<T>(this IList<T> list, Predicate<T> match)
        {
            int count = 0;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (match(list[i]))
                {
                    ++count;
                    list.RemoveAt(i);
                }
            }

            return count;
        }


        public static Task ForEachAsync<T>(this IEnumerable<T> source, int maxConcurrentOperations, Func<T, Task> body)
        {
            return Task.WhenAll(
                from partition in Partitioner.Create(source).GetPartitions(maxConcurrentOperations)
                select Task.Run(async delegate
                {
                    using (partition)
                        while (partition.MoveNext())
                            await body(partition.Current);
                }));
        }

        public static IEnumerable<T> OrderBySequence<T, TId>(this IEnumerable<T> source,
                                                                 IEnumerable<TId> order,
                                                                 Func<T, TId> idSelector)
        {
            var lookup = source.ToLookup(idSelector, t => t);
            var nonMatches = source.ToList();
            foreach (var id in order.Distinct())
            {
                foreach (var t in lookup[id])
                {
                    nonMatches.Remove(t);
                    yield return t;
                }
            }
            foreach (var t in nonMatches)
                yield return t;
        }
    }
}