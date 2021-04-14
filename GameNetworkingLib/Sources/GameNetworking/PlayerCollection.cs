using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GameNetworking {
    public interface IPlayerCollectionListener<TPlayer>
        where TPlayer : class {
        void PlayerStorageDidAdd(TPlayer player);
        void PlayerStorageDidRemove(TPlayer player);
    }

    public interface IReadOnlyPlayerCollection<TKey, TPlayer> : IEnumerable<TPlayer>
        where TKey : IEquatable<TKey>
        where TPlayer : class {
        TPlayer this[TKey key] { get; }
        int count { get; }

        IReadOnlyList<TPlayer> values { get; }

        bool TryGetPlayer(TKey key, out TPlayer player);

        TPlayer FindPlayer(TKey playerId);
        TPlayer FindPlayer(Func<TPlayer, bool> predicate);

        void ForEach<TValue>(Action<TPlayer, TValue> players, TValue value);
    }

    public class PlayerCollection<TKey, TPlayer> : IReadOnlyPlayerCollection<TKey, TPlayer>
        where TKey : IEquatable<TKey>
        where TPlayer : class {
        private readonly ConcurrentDictionary<TKey, TPlayer> playersCollection = new ConcurrentDictionary<TKey, TPlayer>();
        private readonly List<TPlayer> _values = new List<TPlayer>();

        public IReadOnlyList<TPlayer> values => _values;

        public List<IPlayerCollectionListener<TPlayer>> listeners { get; } = new List<IPlayerCollectionListener<TPlayer>>();

        public TPlayer this[TKey key] => playersCollection[key];
        public int count => playersCollection.Count;

        public PlayerCollection() { }

        public bool TryGetPlayer(TKey key, out TPlayer value) {
            return playersCollection.TryGetValue(key, out value);
        }

        public void Add(TKey key, TPlayer player) {
            if (playersCollection.TryAdd(key, player)) {
                _values.Add(player);

                for (int i = 0; i < listeners.Count; i++) {
                    listeners[i].PlayerStorageDidAdd(player);
                }
            }
        }

        public TPlayer Remove(TKey key) {
            if (playersCollection.TryRemove(key, out TPlayer player)) {
                _values.Remove(player);

                for (int i = 0; i < listeners.Count; i++) {
                    listeners[i].PlayerStorageDidRemove(player);
                }
            }
            return player;
        }

        public void Clear() {
            for (int index_p = 0; index_p < values.Count; index_p++) {
                for (int index_l = 0; index_l < listeners.Count; index_l++) {
                    listeners[index_l].PlayerStorageDidRemove(values[index_p]);
                }
            }

            playersCollection.Clear();
            _values.Clear();
        }

        public TPlayer FindPlayer(TKey key) {
            if (playersCollection.TryGetValue(key, out TPlayer player)) {
                return player;
            }
            return null;
        }

        public TPlayer FindPlayer(Func<TPlayer, bool> predicate) {
            TPlayer player = null;
            for (int index = 0; index < values.Count; index++) {
                var value = values[index];
                if (predicate.Invoke(value)) {
                    player = value;
                    break;
                }
            }
            return player;
        }

        public void ForEach<TValue>(Action<TPlayer, TValue> action, TValue value) {
            var p = values;
            var c = p.Count;
            for (int idx = 0; idx < c; idx++) { action(p[idx], value); }
        }

        public IEnumerator<TPlayer> GetEnumerator() {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return values.GetEnumerator();
        }
    }
}