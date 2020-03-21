using System.Collections.Generic;
using GameNetworking.Commons.Models;
using GameNetworking.Commons.Models.Server;
using GameNetworking.Networking.Commons;
using Messages.Models;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace GameNetworking.Commons.Server {
    public abstract class GameServer<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient> :
        INetworkingServer<TSocket, TClient, TNetClient>.IMessagesListener
        where TNetworkingServer : INetworkingServer<TSocket, TClient, TNetClient>
        where TPlayer : NetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {

        public interface IListener {
            void GameServerDidReceiveClientMessage(MessageContainer container, TPlayer player);
        }

        private readonly GameServerMessageRouter<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient> router;

        protected TNetworkingServer networkingServer { get; private set; }
        protected NetworkPlayerCollection<TPlayer, TSocket, TClient, TNetClient> playersStorage { get; private set; }

        public GameServerPingController<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient> pingController { get; }
        public IListener listener { get; set; }

        protected GameServer(IMainThreadDispatcher dispatcher) {
            this.playersStorage = new NetworkPlayerCollection<TPlayer, TSocket, TClient, TNetClient>();

            this.router = new GameServerMessageRouter<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient>(this, dispatcher);

            this.pingController = new GameServerPingController<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient>(this, this.playersStorage);
        }

        public void Start(int port) {
            this.networkingServer.Start(port);
        }

        public void Stop() {
            this.networkingServer.Stop();
        }

        public float GetPing(TPlayer player) {
            return this.pingController.GetPingValue(player);
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

        #region INetworkingServer<ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IMessagesListener

        void INetworkingServer<TSocket, TClient, TNetClient>.IMessagesListener.NetworkingServerDidReadMessage(MessageContainer container, TClient client) {
            var player = this.playersStorage.Find(client);
            this.router.Route(container, player);
        }

        #endregion
    }
}