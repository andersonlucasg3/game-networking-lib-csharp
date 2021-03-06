﻿using System;
using System.Collections.Generic;

namespace GameNetworking.Commons {
    public static class ListExt {
        public static void AddRange<T>(this List<T> list, IEnumerable<T> collection, int count) {
            var index = 0;
            var enumerator = collection.GetEnumerator();
            while (index < count) {
                enumerator.MoveNext();
                list.Add(enumerator.Current);
                index++;
            }
        }

        public static List<T> FindAll<T>(this IReadOnlyList<T> list, Predicate<T> predicate) {
            List<T> values = new List<T>();
            for (int index = 0; index < list.Count; index++) {
                T value = list[index];
                if (predicate.Invoke(value)) { values.Add(value); }
            }
            return values;
        }
    }
}