using System.Collections.Generic;
using Messages.Coders;
using System;

namespace GameNetworking {
    using Networking;
    using Models;
    using Models.Server;
    using Messages;

    public class GameServer {
        private readonly NetworkPlayersStorage playersStorage;

        private readonly GameServerClientAcceptor clientAcceptor;
        private readonly GameServerMessageRouter router;

        private WeakReference weakDelegate;

        internal readonly NetworkingServer networkingServer;
        internal readonly MovementController movementController;

        public IGameServerDelegate Delegate {
            get { return this.weakDelegate?.Target as IGameServerDelegate; }
            set { this.weakDelegate = new WeakReference(value); }
        }

        public GameServer() {
            this.playersStorage = new NetworkPlayersStorage();

            this.networkingServer = new NetworkingServer();

            this.clientAcceptor = new GameServerClientAcceptor(this);
            this.router = new GameServerMessageRouter(this);

            this.movementController = new MovementController(this, this.playersStorage);
        }

        public void Listen(int port) {
            this.networkingServer.Listen(port);
        }

        public void StartGame() {
            this.playersStorage.ForEach((each) => {
                this.networkingServer.Send(new StartGameMessage(), each.Client);
            });
        }

        public void Update() {
            this.networkingServer.AcceptClient();
            this.playersStorage.ForEach((each) => { 
                this.networkingServer.Read(each.Client); 
                this.networkingServer.Flush(each.Client);    
            });
            this.movementController.Update();
        }

        internal void AddPlayer(NetworkPlayer player) {
            this.playersStorage.Add(player);
        }

        internal void RemovePlayer(NetworkPlayer player) {
            this.playersStorage.Remove(player);
        }

        internal NetworkPlayer FindPlayer(NetworkClient client) {
            return this.playersStorage.Find(player => player.Client == client);
        }

        internal NetworkPlayer FindPlayer(int playerId) {
            return this.playersStorage.Find(player => player.PlayerId == playerId);
        }

        internal List<NetworkPlayer> AllPlayers() {
            return this.playersStorage.Players;
        }

        internal void SendBroadcast(IEncodable message) {
            this.networkingServer.SendBroadcast(message, this.playersStorage.ConvertAll(c => c.Client));
        }

        internal void SendBroadcast(IEncodable message, NetworkClient excludeClient) {
            List<NetworkClient> clientList = this.playersStorage.ConvertFindingAll(
                player => player.Client != excludeClient, 
                player => player.Client
            );
            this.networkingServer.SendBroadcast(message, clientList);
        }

        internal void Send(IEncodable message, NetworkClient client) {
            this.networkingServer.Send(message, client);
        }
    }

    internal abstract class BaseServerWorker {
        private readonly WeakReference weakServer;

        protected GameServer Server => this.weakServer?.Target as GameServer;

        protected BaseServerWorker(GameServer server) {
            this.weakServer = new WeakReference(server);
        }
    }
}
