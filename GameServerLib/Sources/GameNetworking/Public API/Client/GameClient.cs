using System;
using System.Collections.Generic;
using Messages.Coders;

namespace GameNetworking {
    using Networking;
    using Models;
    using Models.Client;

    public class GameClient: IGameInstance {
        private readonly NetworkPlayersStorage playersStorage;
        private readonly GameClientConnector connector;
        private readonly GameClientMessageRouter router;
        
        private WeakReference weakDelegate;
        private WeakReference weakInstanceDelegate;

        internal readonly NetworkingClient networkingClient;

        public readonly GameClientMovementController movementController;

        public IGameClientDelegate Delegate {
            get { return this.weakDelegate?.Target as IGameClientDelegate; }
            set { this.weakDelegate = new WeakReference(value); }
        }

        public IGameInstanceDelegate InstanceDelegate {
            get { return this.weakInstanceDelegate?.Target as IGameInstanceDelegate; }
            set { this.weakInstanceDelegate = new WeakReference(value); }
        }

        IMovementController IGameInstance.MovementController { get { return this.movementController; } }

        public GameClient() {
            this.playersStorage = new NetworkPlayersStorage();

            this.networkingClient = new NetworkingClient();

            this.connector = new GameClientConnector(this);
            this.router = new GameClientMessageRouter(this);
            this.movementController = new GameClientMovementController(this, this.playersStorage);
        }

        public void Connect(string host, int port) {
            this.connector.Connect(host, port);
        }

        public void Send(IEncodable message) {
            this.networkingClient.Send(message);
        }

        public void Update() {
            this.movementController.Update();
            
            this.router.Route(this.networkingClient.Read());
            this.networkingClient.Flush();
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
