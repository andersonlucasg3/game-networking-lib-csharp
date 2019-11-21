using System;
using System.Threading;
using System.Collections.Generic;
using Messages.Models;
using Commons;

namespace GameNetworking {
    using Networking;
    using Models;
    using Models.Client;

    public class GameClient : WeakDelegate<IGameClientDelegate>, IGameClientInstance {
        private readonly NetworkPlayersStorage playersStorage;
        private readonly GameClientConnection connector;
        private readonly GameClientMessageRouter router;
        private readonly GameClientPlayersMovementController movementController;

        private WeakReference weakInstanceDelegate;

        internal readonly NetworkingClient networkingClient;

        public float MostRecentPingValue { get; internal set; }

        public IGameClientInstanceDelegate InstanceDelegate {
            get { return this.weakInstanceDelegate?.Target as IGameClientInstanceDelegate; }
            set { this.weakInstanceDelegate = new WeakReference(value); }
        }

        public GameClient() {
            this.playersStorage = new NetworkPlayersStorage();
            this.movementController = new GameClientPlayersMovementController(this, this.playersStorage);

            this.networkingClient = new NetworkingClient();

            this.connector = new GameClientConnection(this);
            this.router = new GameClientMessageRouter(this);
        }

        public void Connect(string host, int port) {
            this.connector.Connect(host, port);
        }

        public void Send(ITypedMessage message) {
            this.networkingClient.Send(message);
        }

        public void Update() {
            this.networkingClient.Update();
            this.movementController.Update();
        }

        internal void AddPlayer(NetworkPlayer player) {
            this.playersStorage.Add(player);
        }

        internal NetworkPlayer FindPlayer(int playerId) {
            return this.playersStorage.Find(player => player.PlayerId == playerId) as NetworkPlayer;
        }

        internal List<GameNetworking.Models.Server.NetworkPlayer> AllPlayers() {
            return this.playersStorage.Players;
        }

        internal void GameClientConnectionDidReceiveMessage(MessageContainer container) {
            this.router.Route(container);
        }
    }
}
