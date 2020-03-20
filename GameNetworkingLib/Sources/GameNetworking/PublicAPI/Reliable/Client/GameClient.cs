using System;
using System.Collections.Generic;
using Messages.Models;
using System.Linq;
using Networking.Commons;
using Networking.Commons.Sockets;
using Networking.Commons.Models;
using GameNetworking.Models.Contract.Client;
using GameNetworking.Commons;
using GameNetworking.Networking;
using GameNetworking.Models;

namespace GameNetworking {
    public class GameClient<TPlayer> where TPlayer : INetworkPlayer, new() {
        public interface IListener {
            void GameClientDidConnect();
            void GameClientConnectDidTimeout();
            void GameClientDidDisconnect();

            void GameClientDidIdentifyLocalPlayer(TPlayer player);

            void GameClientDidReceiveMessage(MessageContainer container);

            void GameClientNetworkPlayerDidDisconnect(TPlayer player);
        }

        private readonly NetworkPlayerCollection<TPlayer> playersStorage;
        private readonly GameClientConnection<TPlayer> connection;
        private readonly GameClientMessageRouter<TPlayer> router;

        internal readonly ReliableNetworkingClient networkingClient;

        public IListener listener { get; set; }

        public GameClient(INetworking<ISocket, INetClient<ISocket>> backend, IMainThreadDispatcher dispatcher) {
            this.playersStorage = new NetworkPlayerCollection<TPlayer>();

            this.networkingClient = new ReliableNetworkingClient(backend);

            this.connection = new GameClientConnection<TPlayer>(this, dispatcher);
            this.router = new GameClientMessageRouter<TPlayer>(this, dispatcher);
        }

        public void Connect(string host, int port) {
            this.connection.Connect(host, port);
        }

        public void Disconnect() {
            this.connection.Disconnect();
        }

        public void Send(ITypedMessage message) {
            this.networkingClient.Send(message);
        }

        public void Update() {
            this.networkingClient.Update();
        }

        public float GetPing(int playerId) {
            var serverPlayer = this.playersStorage[playerId];
            return serverPlayer.mostRecentPingValue;
        }

        public TPlayer FindPlayer(int playerId) {
            if (this.playersStorage.TryGetPlayer(playerId, out TPlayer player)) {
                return player;
            }
            return null;
        }

        public TPlayer FindPlayer(Func<TPlayer, bool> predicate) {
            return this.playersStorage.First(predicate);
        }

        public List<TPlayer> AllPlayers() {
            return this.playersStorage.players;
        }

        internal void AddPlayer(TPlayer n_player) {
            this.playersStorage.Add(n_player);
        }

        internal TPlayer RemovePlayer(int playerId) {
            return this.playersStorage.Remove(playerId);
        }

        internal void GameClientConnectionDidReceiveMessage(MessageContainer container) {
            this.router.Route(container);
        }
    }
}