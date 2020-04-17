using System;
using System.Linq;
using System.Collections.Generic;
using GameNetworking.Commons.Models;
using GameNetworking.Commons.Models.Client;
using GameNetworking.Networking.Commons;
using Messages.Models;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace GameNetworking.Commons.Client {
    public interface IGameClientMessageSender {
        void Send(ITypedMessage message);
    }

    public interface IGameClientListener<TPlayer, TSocket, TClient, TNetClient>
        where TPlayer : class, INetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {
        void GameClientDidConnect();
        void GameClientConnectDidTimeout();
        void GameClientDidDisconnect();

        void GameClientDidIdentifyLocalPlayer(TPlayer player);
        void GameClientDidReceiveMessage(MessageContainer container);
        void GameClientNetworkPlayerDidDisconnect(TPlayer player);
    }

    public interface IGameClient<TPlayer, TSocket, TClient, TNetClient> : IGameClientMessageSender
        where TPlayer : class, INetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {


        double timeOutDelay { get; set; }
        TPlayer localPlayer { get; }

        IGameClientListener<TPlayer, TSocket, TClient, TNetClient> listener { get; set; }

        void Connect(string host, int port);
        void Disconnect();

        void Update();
        float GetPing(int playerId);

        TPlayer FindPlayer(int playerId);
        TPlayer FindPlayer(Func<TPlayer, bool> predicate);
        List<TPlayer> AllPlayers();

        void AddPlayer(TPlayer player);
        TPlayer RemovePlayer(int playerId);
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

        public TPlayer localPlayer { get; private set; }
        public double timeOutDelay { get; set; } = 10F;

        public IGameClientListener<TPlayer, TSocket, TClient, TNetClient> listener { get; set; }

        public GameClient(TNetworkingClient backend, GameClientMessageRouter<TGameClientDerived, TPlayer, TSocket, TClient, TNetClient> router) {
            this.networkingClient = backend;

            this.playersStorage = new NetworkPlayerCollection<TPlayer, TSocket, TClient, TNetClient>();

            this.router = router;
            this.router.Configure(this as TGameClientDerived);
        }

        public abstract void Connect(string host, int port);
        public abstract void Disconnect();

        internal virtual void DidDisconnect() {
            this.localPlayer = null;
        }

        public void Send(ITypedMessage message) {
            this.networkingClient.Send(message);
        }

        public virtual void Update() {
            this.networkingClient.Update();

            if (this.localPlayer == null) { return; }

            var now = TimeUtils.CurrentTime();
            var elapsedTime = now - this.localPlayer.lastReceivedPingRequest;
            if (elapsedTime >= this.timeOutDelay) {
                this.Disconnect();
                this.DidDisconnect();
            }
        }

        public float GetPing(int playerId) {
            if (!this.playersStorage.TryGetPlayer(playerId, out TPlayer player)) { return 0F; }
            return player.mostRecentPingValue;
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
            if (player.isLocalPlayer) { this.localPlayer = player; }

            this.playersStorage.Add(player);
        }

        internal void AddPlayer(TPlayer player) {
            this.self.AddPlayer(player);
        }

        TPlayer IGameClient<TPlayer, TSocket, TClient, TNetClient>.RemovePlayer(int playerId) {
            if (this.localPlayer != null && this.localPlayer.playerId == playerId) { this.localPlayer = null; }
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