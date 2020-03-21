using System;
using System.Collections.Generic;
using GameNetworking.Commons.Models.Server;
using GameNetworking.Commons.Models;
using Networking.Commons.Sockets;
using Networking.Commons.Models;
using GameNetworking.Networking.Commons;
using GameNetworking.Messages.Server;

namespace GameNetworking.Commons.Server {
    public class GameServerPingController<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient> :
        NetworkPlayerCollection<TPlayer, TSocket, TClient, TNetClient>.IListener
        where TPlayer : NetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TNetworkingServer : INetworkingServer<TSocket, TClient, TNetClient>
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {
        private readonly Dictionary<int, PingPlayer<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient>> pingPlayers = new Dictionary<int, PingPlayer<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient>>();
        private PingPlayer<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient>[] pingPlayersArray;

        private readonly GameServer<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient> instance;

        public float PingInterval { get; set; }

        public GameServerPingController(GameServer<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient> instance, NetworkPlayerCollection<TPlayer, TSocket, TClient, TNetClient> storage) {
            this.instance = instance;
            storage.listeners.Add(this);
        }

        public float GetPingValue(TPlayer player) {
            return this.pingPlayers[player.playerId].pingValue;
        }

        internal void Update() {
            if (this.pingPlayersArray == null) { return; }
            PingPlayer<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient> pingPlayer;
            for (int i = 0; i < this.pingPlayersArray.Length; i++) {
                pingPlayer = this.pingPlayersArray[i];
                if (!pingPlayer.pingSent && pingPlayer.canSendNextPing) {
                    pingPlayer.SendingPing();
                    this.instance.Send(new PingRequestMessage(), pingPlayer.player);
                }
            }
        }

        public float PongReceived(TPlayer player) {
            var pingPlayer = this.pingPlayers[player.playerId];
            var pingValue = pingPlayer?.ReceivedPong() ?? 0F;
            player.mostRecentPingValue = pingValue;
            return pingValue;
        }

        void NetworkPlayerCollection<TPlayer, TSocket, TClient, TNetClient>.IListener.PlayerStorageDidAdd(TPlayer player) {
            this.pingPlayers[player.playerId] = new PingPlayer<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient>(player) { pingController = this };
            this.UpdateArray();
        }

        void NetworkPlayerCollection<TPlayer, TSocket, TClient, TNetClient>.IListener.PlayerStorageDidRemove(TPlayer player) {
            if (this.pingPlayers.ContainsKey(player.playerId)) {
                this.pingPlayers.Remove(player.playerId);
                this.UpdateArray();
            }
        }

        private void UpdateArray() {
            this.pingPlayersArray = new List<PingPlayer<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient>>(this.pingPlayers.Values).ToArray();
        }
    }

    internal class PingPlayer<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient>
        where TPlayer : NetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TNetworkingServer : INetworkingServer<TSocket, TClient, TNetClient>
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {
        internal GameServerPingController<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient> pingController { get; set; }

        private float pingSentTime;

        private float pingElapsedTime { get { return CurrentTime() - this.pingSentTime; } }

        internal bool pingSent { get; private set; }
        internal bool canSendNextPing { get { return this.pingElapsedTime > (pingController?.PingInterval ?? 0.5F); } }
        internal float pingValue { get; private set; }

        internal TPlayer player { get; }

        internal PingPlayer(TPlayer instance) {
            this.player = instance;
        }

        internal void SendingPing() {
            this.pingSent = true;
            this.pingSentTime = CurrentTime();
        }

        internal float ReceivedPong() {
            this.pingSent = false;
            this.pingValue = this.pingElapsedTime;
            return this.pingValue;
        }

        public override bool Equals(object obj) {
            if (obj is TPlayer player) {
                return this.player.Equals(player);
            }
            return Equals(this, obj);
        }

        private static float CurrentTime() {
            return (float)TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
