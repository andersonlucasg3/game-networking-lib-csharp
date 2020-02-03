using System;
using System.Collections.Generic;
using Messages.Models;
using Networking;

namespace GameNetworking {
    using Networking;
    using Models;
    using Models.Client;
    using GameNetworking.Commons;

    public class GameClient {
        private readonly NetworkPlayersStorage playersStorage;
        private readonly GameClientConnection connection;
        private readonly GameClientMessageRouter router;

        internal readonly NetworkingClient networkingClient;

        public NetworkPlayer player { get; internal set; }

        public IGameClientListener listener { get; set; }

        public GameClient(INetworking backend, IMainThreadDispatcher dispatcher) {
            this.playersStorage = new NetworkPlayersStorage();

            this.networkingClient = new NetworkingClient(backend);

            this.connection = new GameClientConnection(this, dispatcher);
            this.router = new GameClientMessageRouter(this, dispatcher);
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

        internal void AddPlayer(NetworkPlayer n_player) {
            this.playersStorage.Add(n_player);
        }

        internal NetworkPlayer RemovePlayer(int playerId) {
            return this.playersStorage.Remove(playerId) as NetworkPlayer;
        }

        internal NetworkPlayer FindPlayer(int playerId) {
            return this.playersStorage[playerId] as NetworkPlayer;
        }

        internal List<Models.Server.NetworkPlayer> AllPlayers() {
            return this.playersStorage.players;
        }

        internal void GameClientConnectionDidReceiveMessage(MessageContainer container) {
            this.router.Route(container);
        }
    }
}
