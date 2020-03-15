using System;
using System.Collections.Generic;

namespace GameNetworking.Models {
    using Server;

    public class NetworkPlayersStorage<PlayerType> where PlayerType : NetworkPlayer, new() {
        public interface IListener {
            void PlayerStorageDidAdd(PlayerType player);
            void PlayerStorageDidRemove(PlayerType player);
        }

        private Dictionary<int, PlayerType> playersDict { get; }

        public List<PlayerType> players { get; private set; }

        public List<IListener> listeners { get; set; }

        public PlayerType this[int key] {
            get { return this.playersDict[key]; }
        }

        public NetworkPlayersStorage() {
            this.listeners = new List<IListener>();
            this.playersDict = new Dictionary<int, PlayerType>();
            this.UpdateList();
        }

        private void UpdateList() => this.players = new List<PlayerType>(this.playersDict.Values);

        public bool TryGetPlayer(int key, out PlayerType value) {
            return this.playersDict.TryGetValue(key, out value);
        }

        public void Add(PlayerType player) {
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

        public PlayerType Remove(int playerId) {
            var player = this.playersDict[playerId];
            this.playersDict.Remove(playerId);
            this.UpdateList();
            for (int i = 0; i < this.listeners.Count; i++) {
                this.listeners[i].PlayerStorageDidRemove(player);
            }
            return player;
        }

        public PlayerType Find(NetworkClient client) {
            return this.players.Find(each => each.Equals(client));
        }
    }
}
