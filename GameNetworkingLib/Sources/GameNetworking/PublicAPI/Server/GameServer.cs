using System.Collections.Generic;
using Messages.Models;
using Networking;

namespace GameNetworking {
    using Networking;
    using Models;
    using Models.Server;
    using Commons;
    using Models.Contract.Server;

    public class GameServer<PlayerType> : INetworkingServerListener, INetworkingServerMessagesListener where PlayerType : class, INetworkPlayer, new() {
        public interface IListener {
            void GameServerPlayerDidConnect(PlayerType player);
            void GameServerPlayerDidDisconnect(PlayerType player);
            void GameServerDidReceiveClientMessage(MessageContainer container, PlayerType player);
        }

        private readonly NetworkPlayersStorage<PlayerType> playersStorage;

        private readonly GameServerClientAcceptor<PlayerType> clientAcceptor;
        private readonly GameServerMessageRouter<PlayerType> router;

        internal readonly NetworkingServer networkingServer;

        public readonly GameServerPingController<PlayerType> pingController;

        public IListener listener { get; set; }

        public GameServer(INetworking backend, IMainThreadDispatcher dispatcher) {
            this.playersStorage = new NetworkPlayersStorage<PlayerType>();

            this.networkingServer = new NetworkingServer(backend);

            this.clientAcceptor = new GameServerClientAcceptor<PlayerType>(this, dispatcher);
            this.router = new GameServerMessageRouter<PlayerType>(this, dispatcher);

            this.pingController = new GameServerPingController<PlayerType>(this, this.playersStorage, dispatcher);

            this.networkingServer.listener = this;
            this.networkingServer.messagesListener = this;
        }

        public void Listen(int port) {
            this.networkingServer.Listen(port);
        }

        public void Stop() {
            this.networkingServer.Stop();
        }

        public float GetPing(PlayerType player) {
            return this.pingController.GetPingValue(player);
        }

        public void Disconnect(PlayerType player) {
            this.networkingServer.Disconnect(player.client);
        }

        public void Update() {
            this.networkingServer.Update();
            this.pingController.Update();
        }

        internal void AddPlayer(PlayerType player) {
            this.playersStorage.Add(player);
        }

        internal void RemovePlayer(PlayerType player) {
            this.playersStorage.Remove(player.playerId);
        }

        public PlayerType FindPlayer(int playerId) {
            if (this.playersStorage.TryGetPlayer(playerId, out PlayerType player)) {
                return player;
            }
            return null;
        }

        public List<PlayerType> AllPlayers() {
            return this.playersStorage.players;
        }

        public void SendBroadcast(ITypedMessage message) {
            this.networkingServer.SendBroadcast(message, this.AllPlayers().ConvertAll(c => c.client));
        }

        public void SendBroadcast(ITypedMessage message, PlayerType excludePlayer) {
            PlayerType player;
            for (int i = 0; i < this.playersStorage.players.Count; i++) {
                player = this.playersStorage.players[i];
                if (player != excludePlayer) {
                    this.networkingServer.Send(message, player.client);
                }
            }
        }

        public void Send(ITypedMessage message, PlayerType player) {
            this.networkingServer.Send(message, player.client);
        }

        #region INetworkingServerMessagesDelegate

        void INetworkingServerMessagesListener.NetworkingServerDidReadMessage(MessageContainer container, NetworkClient client) {
            var player = this.playersStorage.Find(client);
            this.router.Route(container, player);
        }

        #endregion

        #region INetworkingServerDelegate

        void INetworkingServerListener.NetworkingServerDidAcceptClient(NetworkClient client) {
            this.clientAcceptor.AcceptClient(client);
        }

        void INetworkingServerListener.NetworkingServerClientDidDisconnect(NetworkClient client) {
            var player = this.playersStorage.Find(client);
            this.clientAcceptor.Disconnect(player);
        }

        #endregion
    }
}
