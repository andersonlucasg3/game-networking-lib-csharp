using System;
using System.Collections.Generic;
using Messages.Models;
using Commons;
using Networking;

namespace GameNetworking {
    using Networking;
    using Models;
    using Models.Client;

    public class GameClient : WeakListener<IGameClientDelegate>, IGameClientInstance {
        private readonly NetworkPlayersStorage playersStorage;
        private readonly GameClientConnection connection;
        private readonly GameClientMessageRouter router;

        private WeakReference weakInstanceDelegate;

        internal readonly NetworkingClient networkingClient;

        public NetworkPlayer Player { get; internal set; }
        public float MostRecentPingValue { get; internal set; }

        public IGameClientInstanceDelegate InstanceDelegate {
            get { return this.weakInstanceDelegate?.Target as IGameClientInstanceDelegate; }
            set { this.weakInstanceDelegate = new WeakReference(value); }
        }

        public GameClient(INetworking backend) {
            this.playersStorage = new NetworkPlayersStorage();

            this.networkingClient = new NetworkingClient(backend);

            this.connection = new GameClientConnection(this);
            this.router = new GameClientMessageRouter(this);
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

        internal void AddPlayer(NetworkPlayer player) {
            this.playersStorage.Add(player);
        }

        internal NetworkPlayer RemovePlayer(int playerId) {
            var player = this.FindPlayer(playerId);
            if (player != null) {
                this.playersStorage.Remove(player);
            }
            return player;
        }

        internal NetworkPlayer FindPlayer(int playerId) {
            return this.playersStorage.Find(player => player.PlayerId == playerId) as NetworkPlayer;
        }

        internal List<Models.Server.NetworkPlayer> AllPlayers() {
            return this.playersStorage.Players;
        }

        internal void GameClientConnectionDidReceiveMessage(MessageContainer container) {
            this.router.Route(container);
        }
    }
}
