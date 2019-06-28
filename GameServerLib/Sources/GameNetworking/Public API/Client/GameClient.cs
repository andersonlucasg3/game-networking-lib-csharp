using System;
using System.Collections.Generic;
using Messages.Coders;

namespace GameNetworking {
    using Networking;
    using Models.Client;

    public sealed class GameClient {
        private readonly List<NetworkPlayer> networkPlayers;
        private readonly GameClientConnector connector;
        private readonly GameClientMessageRouter router;
        private WeakReference weakDelegate;

        internal readonly NetworkingClient networkingClient;

        public IGameClientDelegate Delegate {
            get { return this.weakDelegate?.Target as IGameClientDelegate; }
            set { this.weakDelegate = new WeakReference(value); }
        }

        public GameClient() {
            this.networkPlayers = new List<NetworkPlayer>();

            this.networkingClient = new NetworkingClient();

            this.connector = new GameClientConnector(this);
            this.router = new GameClientMessageRouter(this);
        }

        public void Connect(string host, int port) {
            this.connector.Connect(host, port);
        }

        public void Send(IEncodable message) {
            this.networkingClient.Send(message);
        }

        public void Update() {
            this.router.Route(this.networkingClient.Read());
            this.networkingClient.Flush();
        }

        internal void AddPlayer(NetworkPlayer player) {
            this.networkPlayers.Add(player);
        }

        internal NetworkPlayer FindPlayer(int playerId) {
            return this.networkPlayers.Find(player => player.PlayerId == playerId);
        }
    }

    internal abstract class BaseClientWorker {
        private readonly WeakReference weakClient;

        protected GameClient Client => this.weakClient?.Target as GameClient;

        protected BaseClientWorker(GameClient client) {
            this.weakClient = new WeakReference(client);
        }
    }
}
