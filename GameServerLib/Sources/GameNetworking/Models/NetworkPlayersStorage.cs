using System;
using System.Collections.Generic;
using Commons;

namespace GameNetworking.Models {
    using Server;

    public interface INetworkPlayerStorageChangeDelegate {
        void PlayerStorageDidAdd(NetworkPlayer player);
        void PlayerStorageDidRemove(NetworkPlayer player);
    }

    public class NetworkPlayersStorage : WeakDelegates<INetworkPlayerStorageChangeDelegate> {
        private Dictionary<int, NetworkPlayer> playersDict { get; }

        public List<NetworkPlayer> players { get; private set; }

        public NetworkPlayer this[int key] {
            get { return this.playersDict[key]; }
        }

        public NetworkPlayersStorage() {
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
                this.ForEach((INetworkPlayerStorageChangeDelegate del) => {
                    del.PlayerStorageDidAdd(player);
                });
            }
        }

        public NetworkPlayer Remove(int playerId) {
            var player = this.playersDict[playerId];
            this.playersDict.Remove(playerId);
            this.UpdateList();
            this.ForEach((INetworkPlayerStorageChangeDelegate del) => {
                del.PlayerStorageDidRemove(player);
            });
            return player;
        }

        public NetworkPlayer Find(NetworkClient client) {
            return this.players.Find(each => each.Equals(client));
        }
    }
}
