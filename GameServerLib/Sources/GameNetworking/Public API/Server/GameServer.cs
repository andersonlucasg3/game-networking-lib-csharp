using UnityEngine;
using System.Collections.Generic;
using Messages.Coders;
using System;

namespace GameNetworking {
    using Networking;
    using Models;
    using Messages;

    public class GameServer {
        private readonly List<ClientPlayerPair> connectedPlayers;

        private readonly GameServerClientAcceptor clientAcceptor;
        private readonly GameServerMessageRouter router;

        private WeakReference weakDelegate;

        internal readonly NetworkingServer networkingServer;

        public IGameServerDelegate Delegate {
            get { return this.weakDelegate?.Target as IGameServerDelegate; }
            set { this.weakDelegate = new WeakReference(value); }
        }

        public GameServer() {
            this.connectedPlayers = new List<ClientPlayerPair>();

            this.networkingServer = new NetworkingServer();

            this.clientAcceptor = new GameServerClientAcceptor(this);
            this.router = new GameServerMessageRouter(this);
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
        }

        internal void AddPair(ClientPlayerPair pair) {
            this.connectedPlayers.Add(pair);
        }

        internal ClientPlayerPair FindPair(NetworkClient client) {
            return this.connectedPlayers.Find(pair => pair == client);
        }

        internal void BroadcastMessage(IEncodable message) {
            this.networkingServer.SendBroadcast(message, this.connectedPlayers.ConvertAll(c => c.Client));
        }

        internal void BroadcastMessage(IEncodable message, NetworkClient excludeClient) {
            List<NetworkClient> all = new List<NetworkClient>();
            foreach (var x in this.connectedPlayers) { if (x != excludeClient) { all.Add(x.Client); } }
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
