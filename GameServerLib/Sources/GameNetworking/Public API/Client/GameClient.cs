using System;
using System.Collections.Generic;
using Messages.Models;

namespace GameNetworking {
    using Networking;
    using Models;
    using Models.Client;

    public class GameClient: IGameInstance {
        private readonly NetworkPlayersStorage playersStorage;
        private readonly GameClientConnector connector;
        private readonly GameClientMessageRouter router;
        private readonly GameClientPlayersMovementController movementController;
        
        private WeakReference weakDelegate;
        private WeakReference weakInstanceDelegate;

        internal readonly NetworkingClient networkingClient;

        public IGameClientDelegate Delegate {
            get { return this.weakDelegate?.Target as IGameClientDelegate; }
            set { this.weakDelegate = new WeakReference(value); }
        }

        public IGameInstanceDelegate InstanceDelegate {
            get { return this.weakInstanceDelegate?.Target as IGameInstanceDelegate; }
            set { this.weakInstanceDelegate = new WeakReference(value); }
        }

        public GameClient() {
            this.playersStorage = new NetworkPlayersStorage();
            this.movementController = new GameClientPlayersMovementController(this, this.playersStorage);

            this.networkingClient = new NetworkingClient();

            this.connector = new GameClientConnector(this);
            this.router = new GameClientMessageRouter(this);
        }

        public void Connect(string host, int port) {
            this.connector.Connect(host, port);
        }

        public void Send(ITypedMessage message) {
            this.networkingClient.Send(message);
        }

        public void Update() {
            this.router.Route(this.networkingClient.Read());
            this.networkingClient.Flush();

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
    }
}
