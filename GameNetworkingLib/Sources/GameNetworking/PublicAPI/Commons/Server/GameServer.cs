using System.Collections.Generic;
using GameNetworking.Commons.Models;
using GameNetworking.Commons.Models.Server;
using GameNetworking.Networking.Commons;
using Messages.Models;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace GameNetworking.Commons.Server {
    public interface IGameServer<TPlayer, TSocket, TClient, TNetClient>
        where TPlayer : INetworkPlayer<TSocket, TClient, TNetClient>
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {

        public interface IListener {
            void GameServerPlayerDidConnect(TPlayer player);
            void GameServerPlayerDidDisconnect(TPlayer player);
            void GameServerDidReceiveClientMessage(MessageContainer container, TPlayer player);
        }

        IListener listener { get; set; }

        public IGameServerPingController<TPlayer, TSocket, TClient, TNetClient> pingController { get; }

        void Start(string host, int port);
        void Stop();

        void Update();

        TPlayer FindPlayer(int playerId);
        List<TPlayer> AllPlayers();
        void Send(ITypedMessage message, TPlayer player);
        void SendBroadcast(ITypedMessage message);
        void SendBroadcast(ITypedMessage message, TPlayer excludePlayer);

        internal void AddPlayer(TPlayer player);
        internal void RemovePlayer(TPlayer player);
    }

    public abstract class GameServer<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient, TClientAcceptor, TGameServerDerived> : IGameServer<TPlayer, TSocket, TClient, TNetClient>,
        GameServerClientAcceptor<TGameServerDerived, TNetworkingServer, TPlayer, TSocket, TClient, TNetClient>.IListener,
        INetworkingServer<TSocket, TClient, TNetClient>.IListener,
        INetworkingServer<TSocket, TClient, TNetClient>.IMessagesListener
        where TNetworkingServer : INetworkingServer<TSocket, TClient, TNetClient>
        where TPlayer : class, INetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient>
        where TClientAcceptor : GameServerClientAcceptor<TGameServerDerived, TNetworkingServer, TPlayer, TSocket, TClient, TNetClient>, new()
        where TGameServerDerived : GameServer<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient, TClientAcceptor, TGameServerDerived> { 

        private readonly GameServerMessageRouter<TGameServerDerived, TNetworkingServer, TPlayer, TSocket, TClient, TNetClient> router;
        private readonly TClientAcceptor clientAcceptor;

        private IGameServer<TPlayer, TSocket, TClient, TNetClient> self => this;

        protected TNetworkingServer networkingServer { get; private set; }
        protected NetworkPlayerCollection<TPlayer, TSocket, TClient, TNetClient> playersStorage { get; private set; }

        public IGameServerPingController<TPlayer, TSocket, TClient, TNetClient> pingController { get; }
        public IGameServer<TPlayer, TSocket, TClient, TNetClient>.IListener listener { get; set; }

        protected GameServer(TNetworkingServer server, GameServerMessageRouter<TGameServerDerived, TNetworkingServer, TPlayer, TSocket, TClient, TNetClient> router) {
            this.networkingServer = server;

            this.playersStorage = new NetworkPlayerCollection<TPlayer, TSocket, TClient, TNetClient>();
            this.pingController = new GameServerPingController<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient>(this, this.playersStorage);

            this.router = router;
            this.router.Configure(this as TGameServerDerived);

            this.networkingServer.messagesListener = this;

            this.clientAcceptor = new TClientAcceptor() { listener = this };
        }

        public void Start(string host, int port) {
            this.networkingServer.Start(host, port);
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

        void IGameServer<TPlayer, TSocket, TClient, TNetClient>.AddPlayer(TPlayer player) {
            this.playersStorage.Add(player);
        }

        internal void AddPlayer(TPlayer player) {
            this.self.AddPlayer(player);
        }

        void IGameServer<TPlayer, TSocket, TClient, TNetClient>.RemovePlayer(TPlayer player) {
            this.playersStorage.Remove(player.playerId);
        }

        internal void RemovePlayer(TPlayer player) {
            this.self.RemovePlayer(player);
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

        void INetworkingServer<TSocket, TClient, TNetClient>.IListener.NetworkingServerDidAcceptClient(TClient client) { 
            this.clientAcceptor.AcceptClient(client);
        }

        void INetworkingServer<TSocket, TClient, TNetClient>.IListener.NetworkingServerClientDidDisconnect(TClient client) {
            var player = this.playersStorage.Find(client);
            this.clientAcceptor.Disconnect(player);
        }

        void INetworkingServer<TSocket, TClient, TNetClient>.IMessagesListener.NetworkingServerDidReadMessage(MessageContainer container, TClient client) {
            var player = this.playersStorage.Find(client);
            this.router.Route(container, player);
        }

        void GameServerClientAcceptor<TGameServerDerived, TNetworkingServer, TPlayer, TSocket, TClient, TNetClient>.IListener.ClientAcceptorPlayerDidConnect(TPlayer player) {
            this.listener?.GameServerPlayerDidConnect(player);
        }

        void GameServerClientAcceptor<TGameServerDerived, TNetworkingServer, TPlayer, TSocket, TClient, TNetClient>.IListener.ClientAcceptorPlayerDidDisconnect(TPlayer player) {
            this.listener?.GameServerPlayerDidDisconnect(player);
        }

        #endregion
    }
}