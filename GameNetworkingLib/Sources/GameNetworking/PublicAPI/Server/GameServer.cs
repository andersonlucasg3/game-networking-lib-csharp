using System.Collections.Generic;
using Messages.Models;
using Networking;

namespace GameNetworking {
    using Networking;
    using Models;
    using Models.Server;
    using Commons;

    public class GameServer<TPlayer> : INetworkingServerListener, INetworkingServerMessagesListener where TPlayer : NetworkPlayer, new() {
        public interface IListener {
            void GameServerPlayerDidConnect(TPlayer player);
            void GameServerPlayerDidDisconnect(TPlayer player);
            void GameServerDidReceiveClientMessage(MessageContainer container, TPlayer player);
        }

        private readonly NetworkPlayersCollection<TPlayer> playersStorage;

        private readonly GameServerClientAcceptor<TPlayer> clientAcceptor;
        private readonly GameServerMessageRouter<TPlayer> router;

        internal readonly NetworkingServer networkingServer;

        public GameServerPingController<TPlayer> pingController { get; }

        public IListener listener { get; set; }

        public GameServer(INetworking backend, IMainThreadDispatcher dispatcher) {
            this.playersStorage = new NetworkPlayersCollection<TPlayer>();

            this.networkingServer = new NetworkingServer(backend);

            this.clientAcceptor = new GameServerClientAcceptor<TPlayer>(this, dispatcher);
            this.router = new GameServerMessageRouter<TPlayer>(this, dispatcher);

            this.pingController = new GameServerPingController<TPlayer>(this, this.playersStorage, dispatcher);

            this.networkingServer.listener = this;
            this.networkingServer.messagesListener = this;
        }

        public void Listen(int port) {
            this.networkingServer.Listen(port);
        }

        public void Stop() {
            this.networkingServer.Stop();
        }

        public float GetPing(TPlayer player) {
            return this.pingController.GetPingValue(player);
        }

        public void Disconnect(TPlayer player) {
            this.networkingServer.Disconnect(player.client);
        }

        public void Update() {
            this.networkingServer.Update();
            this.pingController.Update();
        }

        internal void AddPlayer(TPlayer player) {
            this.playersStorage.Add(player);
        }

        internal void RemovePlayer(TPlayer player) {
            this.playersStorage.Remove(player.playerId);
        }

        public TPlayer FindPlayer(int playerId) {
            if (this.playersStorage.TryGetPlayer(playerId, out TPlayer player)) {
                return player;
            }
            return null;
        }

        public List<TPlayer> AllPlayers() {
            return this.playersStorage.players;
        }

        public void SendBroadcast(ITypedMessage message) {
            this.networkingServer.SendBroadcast(message, this.AllPlayers().ConvertAll(c => c.client));
        }

        public void SendBroadcast(ITypedMessage message, TPlayer excludePlayer) {
            TPlayer player;
            for (int i = 0; i < this.playersStorage.players.Count; i++) {
                player = this.playersStorage.players[i];
                if (player != excludePlayer) {
                    this.networkingServer.Send(message, player.client);
                }
            }
        }

        public void Send(ITypedMessage message, TPlayer player) {
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
