using System;
using System.Collections.Generic;

namespace GameNetworking.Models {
    using System.Collections;
    using GameNetworking.Models.Server;

    public class NetworkPlayersCollection<TPlayer> : IEnumerable<TPlayer> where TPlayer : NetworkPlayer, new() {
        public interface IListener {
            void PlayerStorageDidAdd(TPlayer player);
            void PlayerStorageDidRemove(TPlayer player);
        }

        private Dictionary<int, TPlayer> playersDict { get; }

        public List<TPlayer> players { get; private set; }

        public List<IListener> listeners { get; }

        public TPlayer this[int key] {
            get { return this.playersDict[key]; }
        }

        public NetworkPlayersCollection() {
            this.listeners = new List<IListener>();
            this.playersDict = new Dictionary<int, TPlayer>();
            this.UpdateList();
        }

        private void UpdateList() => this.players = new List<TPlayer>(this.playersDict.Values);

        public bool TryGetPlayer(int key, out TPlayer value) {
            return this.playersDict.TryGetValue(key, out value);
        }

        public void Add(TPlayer player) {
            if (player == null) { return; }

            if (this.playersDict.ContainsKey(player.playerId)) {
                throw new OperationCanceledException($"Player id {player.playerId} already present.");
            } else {
                this.playersDict[player.playerId] = player;
                this.UpdateList();
                for (int i = 0; i < this.listeners.Count; i++) {
                    this.listeners[i].PlayerStorageDidAdd(player);
                }
            }
        }

        public TPlayer Remove(int playerId) {
            var player = this.playersDict[playerId];
            this.playersDict.Remove(playerId);
            this.UpdateList();
            for (int i = 0; i < this.listeners.Count; i++) {
                this.listeners[i].PlayerStorageDidRemove(player);
            }
            return player;
        }

        public TPlayer Find(NetworkClient client) {
            return this.players.Find(each => each.Equals(client));
        }

        public IEnumerator<TPlayer> GetEnumerator() {
            return this.players.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.players.GetEnumerator();
        }
    }
}