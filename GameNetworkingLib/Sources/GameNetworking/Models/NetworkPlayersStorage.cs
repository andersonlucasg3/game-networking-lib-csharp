using System;
using System.Collections.Generic;

namespace GameNetworking.Models {
    using Server;

    public interface INetworkPlayerStorageChangeListener {
        void PlayerStorageDidAdd(NetworkPlayer player);
        void PlayerStorageDidRemove(NetworkPlayer player);
    }

    public class NetworkPlayersStorage {
        private Dictionary<int, NetworkPlayer> playersDict { get; }

        public List<NetworkPlayer> players { get; private set; }

        public List<INetworkPlayerStorageChangeListener> listeners { get; set; }

        public NetworkPlayer this[int key] {
            get { return this.playersDict[key]; }
        }

        public NetworkPlayersStorage() {
            this.listeners = new List<INetworkPlayerStorageChangeListener>();
            this.playersDict = new Dictionary<int, NetworkPlayer>();
            this.UpdateList();
        }

        private void UpdateList() => this.players = new List<NetworkPlayer>(this.playersDict.Values);

        public bool TryGetPlayer(int key, out NetworkPlayer value) {
            return this.playersDict.TryGetValue(key, out value);
        }

        public void Add(NetworkPlayer player) {
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

        public NetworkPlayer Remove(int playerId) {
            var player = this.playersDict[playerId];
            this.playersDict.Remove(playerId);
            this.UpdateList();
            for (int i = 0; i < this.listeners.Count; i++) {
                this.listeners[i].PlayerStorageDidRemove(player);
            }
            return player;
        }

        public NetworkPlayer Find(NetworkClient client) {
            return this.players.Find(each => each.Equals(client));
        }
    }
}
