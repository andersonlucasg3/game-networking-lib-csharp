using System;
using System.Collections.Generic;

namespace GameNetworking.Commons
{
    public static class ListExt
    {
        public static void AddRange<TItem>(this List<TItem> list, IReadOnlyList<TItem> collection, int count)
        {
            for (int index = 0; index < count; index++)
            {
                list.Add(collection[index]);
            }
        }

        public static List<TItem> FindAll<TItem>(this IReadOnlyList<TItem> list, Predicate<TItem> predicate)
        {
            using (PooledList<TItem> values = PooledList<TItem>.Rent())
            {
                for (var index = 0; index < list.Count; index++)
                {
                    var value = list[index];
                    if (predicate.Invoke(value)) values.Add(value);
                }

                return values;
            }
        }
    }
}
