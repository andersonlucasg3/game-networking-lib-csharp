using System;
using System.Linq;
using System.Collections.Generic;
using GameNetworking.Commmons.Client;
using GameNetworking.Commons.Models;
using GameNetworking.Commons.Models.Contract.Client;
using GameNetworking.Networking.Commons;
using Messages.Models;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace GameNetworking.Commons.Client {
    public abstract class GameClient<TNetworkingClient, TPlayer, TSocket, TClient, TNetClient>
        where TNetworkingClient : INetworkingClient<TSocket, TClient, TNetClient>
        where TPlayer : class, INetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {
        public interface IListener {
            void GameClientDidIdentifyLocalPlayer(TPlayer player);
            void GameClientDidReceiveMessage(MessageContainer container);
            void GameClientNetworkPlayerDidDisconnect(TPlayer player);
        }

        protected NetworkPlayerCollection<TPlayer, TSocket, TClient, TNetClient> playersStorage { get; private set; }
        protected GameClientMessageRouter<TNetworkingClient, TPlayer, TSocket, TClient, TNetClient> router { get; private set; }

        internal TNetworkingClient networkingClient { get; private set; }

        public IListener listener { get; set; }

        public GameClient(TNetworkingClient backend, IMainThreadDispatcher dispatcher) {
            this.networkingClient = backend;

            this.playersStorage = new NetworkPlayerCollection<TPlayer, TSocket, TClient, TNetClient>();

            this.router = new GameClientMessageRouter<TNetworkingClient, TPlayer, TSocket, TClient, TNetClient>(this, dispatcher);
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