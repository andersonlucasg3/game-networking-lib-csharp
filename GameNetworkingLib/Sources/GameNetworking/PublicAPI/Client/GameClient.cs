using System.Collections.Generic;
using Messages.Models;
using Networking;

namespace GameNetworking {
    using Networking;
    using Models;
    using GameNetworking.Commons;
    using GameNetworking.Models.Client;

    public class GameClient<PlayerType> where PlayerType : NetworkPlayer, new() {
        public interface IListener {
            void GameClientDidConnect();
            void GameClientConnectDidTimeout();
            void GameClientDidDisconnect();

            void GameClientDidReceiveMessage(MessageContainer container);

            void GameClientNetworkPlayerDidDisconnect(PlayerType player);
        }

        private readonly NetworkPlayersStorage<PlayerType> playersStorage;
        private readonly GameClientConnection<PlayerType> connection;
        private readonly GameClientMessageRouter<PlayerType> router;

        internal readonly NetworkingClient networkingClient;

        public PlayerType player { get; internal set; }

        public IListener listener { get; set; }

        public GameClient(INetworking backend, IMainThreadDispatcher dispatcher) {
            this.playersStorage = new NetworkPlayersStorage<PlayerType>();

            this.networkingClient = new NetworkingClient(backend);

            this.connection = new GameClientConnection<PlayerType>(this, dispatcher);
            this.router = new GameClientMessageRouter<PlayerType>(this, dispatcher);
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

        public PlayerType FindPlayer(int playerId) {
            return this.playersStorage[playerId];
        }

        public List<PlayerType> AllPlayers() {
            return this.playersStorage.players;
        }

        internal void AddPlayer(PlayerType n_player) {
            this.playersStorage.Add(n_player);
        }

        internal PlayerType RemovePlayer(int playerId) {
            return this.playersStorage.Remove(playerId);
        }

        internal void GameClientConnectionDidReceiveMessage(MessageContainer container) {
            this.router.Route(container);
        }
    }
}