using System.Collections.Generic;
using Messages.Models;
using Networking;

namespace GameNetworking {
    using Networking;
    using Models;
    using Models.Server;
    using Messages;

    public class GameServer : IGameInstance, INetworkingServerListener, INetworkingServerMessagesListener {
        private readonly NetworkPlayersStorage playersStorage;

        private readonly GameServerClientAcceptor clientAcceptor;
        private readonly GameServerMessageRouter router;

        internal readonly NetworkingServer networkingServer;

        public readonly GameServerPingController pingController;
        public readonly GameServerSyncController syncController;

        public IGameServerListener listener { get; set; }

        public GameServer(INetworking backend) {
            this.playersStorage = new NetworkPlayersStorage();

            this.networkingServer = new NetworkingServer(backend);

            this.clientAcceptor = new GameServerClientAcceptor(this);
            this.router = new GameServerMessageRouter(this);

            this.syncController = new GameServerSyncController(this, this.playersStorage);
            this.pingController = new GameServerPingController(this, this.playersStorage);

            this.networkingServer.listener = this;
            this.networkingServer.messagesListener = this;
        }

        public void Listen(int port) {
            this.networkingServer.Listen(port);
        }

        public void Stop() {
            this.networkingServer.Stop();
        }

        public void StartGame() {
            NetworkPlayer player;
            var message = new StartGameMessage();
            for (int i = 0; i < this.playersStorage.players.Count; i++) {
                player = this.playersStorage.players[i];
                this.networkingServer.Send(message, player.client);
            }
        }

        public float GetPing(NetworkPlayer player) {
            return this.pingController.GetPingValue(player);
        }

        public void Disconnect(NetworkPlayer player) {
            this.networkingServer.Disconnect(player.client);
        }

        public void Update() {
            this.networkingServer.Update();
            this.syncController.Update();
            this.pingController.Update();
        }

        internal void AddPlayer(NetworkPlayer player) {
            this.playersStorage.Add(player);
        }

        internal void RemovePlayer(NetworkPlayer player) {
            this.playersStorage.Remove(player.playerId);
        }

        public NetworkPlayer FindPlayer(int playerId) {
            if (this.playersStorage.TryGetPlayer(playerId, out NetworkPlayer player)) {
                return player;
            }
            return null;
        }

        public List<NetworkPlayer> AllPlayers() {
            return this.playersStorage.players;
        }

        internal void SendBroadcast(ITypedMessage message) {
            this.networkingServer.SendBroadcast(message, this.AllPlayers().ConvertAll(c => c.client));
        }

        internal void SendBroadcast(ITypedMessage message, NetworkPlayer excludePlayer) {
            NetworkPlayer player;
            for (int i = 0; i < this.playersStorage.players.Count; i++) {
                player = this.playersStorage.players[i];
                if (player != excludePlayer) {
                    this.networkingServer.Send(message, player.client);
                }
            }
        }

        internal void Send(ITypedMessage message, NetworkClient client) {
            this.networkingServer.Send(message, client);
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
