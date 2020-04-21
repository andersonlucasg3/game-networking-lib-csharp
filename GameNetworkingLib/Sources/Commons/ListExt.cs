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
    }
}