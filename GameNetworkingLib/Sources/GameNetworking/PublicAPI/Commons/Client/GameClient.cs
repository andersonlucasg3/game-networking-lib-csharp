using System;
using System.Linq;
using System.Collections.Generic;
using GameNetworking.Commons.Models;
using GameNetworking.Commons.Models.Client;
using GameNetworking.Networking.Commons;
using Messages.Models;
using Networking.Commons.Models;
using Networking.Commons.Sockets;
using Logging;

namespace GameNetworking.Commons.Client {
    public interface IGameClientMessageSender {
        void Send(ITypedMessage message);
    }

    public interface IGameClient<TPlayer, TSocket, TClient, TNetClient> : IGameClientMessageSender
        where TPlayer : class, INetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {

        public interface IListener {
            void GameClientDidConnect();
            void GameClientConnectDidTimeout();
            void GameClientDidDisconnect();

            void GameClientDidIdentifyLocalPlayer(TPlayer player);
            void GameClientDidReceiveMessage(MessageContainer container);
            void GameClientNetworkPlayerDidDisconnect(TPlayer player);
        }

        IListener listener { get; set; }

        void Connect(string host, int port);
        void Disconnect();

        void Update();
        float GetPing(int playerId);
        
        TPlayer FindPlayer(int playerId);
        TPlayer FindPlayer(Func<TPlayer, bool> predicate);
        List<TPlayer> AllPlayers();

        internal void AddPlayer(TPlayer player);
        internal TPlayer RemovePlayer(int playerId);
    }

    public abstract class GameClient<TNetworkingClient, TPlayer, TSocket, TClient, TNetClient, TGameClientDerived> : IGameClient<TPlayer, TSocket, TClient, TNetClient>
        where TNetworkingClient : INetworkingClient<TSocket, TClient, TNetClient>
        where TPlayer : class, INetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient>
        where TGameClientDerived : GameClient<TNetworkingClient, TPlayer, TSocket, TClient, TNetClient, TGameClientDerived> {

        private IGameClient<TPlayer, TSocket, TClient, TNetClient> self => this;

        protected NetworkPlayerCollection<TPlayer, TSocket, TClient, TNetClient> playersStorage { get; private set; }
        protected GameClientMessageRouter<TGameClientDerived, TPlayer, TSocket, TClient, TNetClient> router { get; private set; }

        internal TNetworkingClient networkingClient { get; private set; }

        public IGameClient<TPlayer, TSocket, TClient, TNetClient>.IListener listener { get; set; }

        public GameClient(TNetworkingClient backend, GameClientMessageRouter<TGameClientDerived, TPlayer, TSocket, TClient, TNetClient> router) {
            this.networkingClient = backend;

            this.playersStorage = new NetworkPlayerCollection<TPlayer, TSocket, TClient, TNetClient>();

            this.router = router;
            this.router.Configure(this as TGameClientDerived);
        }

        public abstract void Connect(string host, int port);
        public abstract void Disconnect();

        public void Send(ITypedMessage message) {
            Logger.Log($"Sending {message} to server");
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

        void IGameClient<TPlayer, TSocket, TClient, TNetClient>.AddPlayer(TPlayer player) {
            this.playersStorage.Add(player);
        }

        internal void AddPlayer(TPlayer player) {
            this.self.AddPlayer(player);
        }

        TPlayer IGameClient<TPlayer, TSocket, TClient, TNetClient>.RemovePlayer(int playerId) {
            return this.playersStorage.Remove(playerId);
        }

        internal TPlayer RemovePlayer(int playerId) {
            return this.self.RemovePlayer(playerId);
        }

        internal void GameClientConnectionDidReceiveMessage(MessageContainer container) {
            this.router.Route(container);
        }
    }
}