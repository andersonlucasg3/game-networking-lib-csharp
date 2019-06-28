using System.Collections.Generic;
using Messages.Coders;
using System;

namespace GameNetworking {
    using Networking;
    using Models;
    using Models.Server;
    using Messages;

    public class GameServer {
        private readonly List<NetworkPlayer> connectedPlayers;

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
            this.connectedPlayers = new List<NetworkPlayer>();

            this.networkingServer = new NetworkingServer();

            this.clientAcceptor = new GameServerClientAcceptor(this);
            this.router = new GameServerMessageRouter(this);

            this.movementController = new MovementController(this) {
                Players = this.connectedPlayers
            };
        }

        public void Listen(int port) {
            this.networkingServer.Listen(port);
        }

        public void StartGame() {
            this.connectedPlayers.ForEach((each) => {
                this.networkingServer.Send(new StartGameMessage(), each.Client);
            });
        }

        public void Update() {
            this.networkingServer.AcceptClient();
            this.connectedPlayers.ForEach((each) => { this.networkingServer.Read(each.Client); });
            this.connectedPlayers.ForEach((each) => { this.networkingServer.Flush(each.Client); });
            this.movementController.Update();
        }

        internal void AddPlayer(NetworkPlayer player) {
            this.connectedPlayers.Add(player);
        }

        internal NetworkPlayer FindPlayer(NetworkClient client) {
            return this.connectedPlayers.Find(player => player.Client == client);
        }

        internal void SendBroadcast(IEncodable message) {
            this.networkingServer.SendBroadcast(message, this.connectedPlayers.ConvertAll(c => c.Client));
        }

        internal void SendBroadcast(IEncodable message, NetworkClient excludeClient) {
            List<NetworkClient> all = new List<NetworkClient>();
            foreach (var x in this.connectedPlayers) { if (x.Client != excludeClient) { all.Add(x.Client); } }
            this.networkingServer.SendBroadcast(message, all);
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
