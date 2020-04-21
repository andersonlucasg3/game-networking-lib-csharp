using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GameNetworking {
    public interface IPlayerCollectionListener<TPlayer>
        where TPlayer : class {
        void PlayerStorageDidAdd(TPlayer player);
        void PlayerStorageDidRemove(TPlayer player);
    }

    public interface IReadOnlyPlayerCollection<TKey, TPlayer> : IEnumerable<TPlayer>
        where TKey : struct
        where TPlayer : class { 
        TPlayer this[TKey key] { get; }
        int count { get; }

        List<TPlayer> players { get; }

        bool TryGetPlayer(TKey key, out TPlayer player);

        TPlayer FindPlayer(TKey playerId);
        TPlayer FindPlayer(Func<TPlayer, bool> predicate);

        void ForEach(Action<TPlayer> players);
    }

    public class PlayerCollection<TKey, TPlayer> : IReadOnlyPlayerCollection<TKey, TPlayer>
        where TKey : struct
        where TPlayer : class {
        private readonly Dictionary<TKey, TPlayer> playersDict;

        public List<TPlayer> players { get; private set; }

        public List<IPlayerCollectionListener<TPlayer>> listeners { get; }

        public TPlayer this[TKey key] => this.playersDict[key];
        public int count => this.players.Count;

        public PlayerCollection() {
            this.playersDict = new Dictionary<TKey, TPlayer>();
            this.listeners = new List<IPlayerCollectionListener<TPlayer>>();
            this.UpdateList();
        }

        private void UpdateList() => this.players = new List<TPlayer>(this.playersDict.Values);

        public bool TryGetPlayer(TKey key, out TPlayer value) {
            return this.playersDict.TryGetValue(key, out value);
        }

        public void Add(TKey key, TPlayer player) {
            if (!this.playersDict.ContainsKey(key)) {
                this.playersDict[key] = player;
                this.UpdateList();
                for (int i = 0; i < this.listeners.Count; i++) {
                    this.listeners[i].PlayerStorageDidAdd(player);
                }
            }
        }

        public TPlayer Remove(TKey key) {
            if (!this.playersDict.ContainsKey(key)) { return null; }

            var player = this.playersDict[key];
            this.playersDict.Remove(key);
            this.UpdateList();
            for (int i = 0; i < this.listeners.Count; i++) {
                this.listeners[i].PlayerStorageDidRemove(player);
            }
            return player;
        }

        public void Clear() {
            for (int index_p = 0; index_p < this.players.Count; index_p++) {
                for (int index_l = 0; index_l < this.listeners.Count; index_l++) {
                    this.listeners[index_l].PlayerStorageDidRemove(players[index_p]);
                }
            }

            this.playersDict.Clear();
            this.UpdateList();
        }

        public TPlayer FindPlayer(TKey key) {
            if (this.playersDict.TryGetValue(key, out TPlayer player)) {
                return player;
            }
            return null;
        }

        public TPlayer FindPlayer(Func<TPlayer, bool> predicate) {
            return this.players.First(predicate);
        }

        public void ForEach(Action<TPlayer> action) {
            var p = this.players;
            var c = p.Count;
            for (int idx = 0; idx < c; idx++) { action(p[idx]); }
        }

        public IEnumerator<TPlayer> GetEnumerator() {
            return this.players.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.players.GetEnumerator();
        }
    }
}