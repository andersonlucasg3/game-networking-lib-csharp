using System;
using System.Collections.Generic;
using Messages.Coders;
using Messages.Models;

namespace GameNetworking {
    using Networking;
    using Models;

    public sealed class GameClient {
        private readonly NetworkingClient networkingClient;
        private readonly List<NetworkPlayer> networkPlayers;

        private readonly GameClientConnector connector;
        private readonly GameClientMessageRouter router;

        private WeakReference weakDelegate;

        public IGameClientDelegate Delegate {
            get { return this.weakDelegate?.Target as IGameClientDelegate; }
            set { this.weakDelegate = new WeakReference(value); }
        }

        public GameClient() {
            this.networkPlayers = new List<NetworkPlayer>();

            this.networkingClient = new NetworkingClient();

            this.connector = new GameClientConnector(this.networkingClient) {
                Delegate = new ConnectorDelegate(this)
            };
            this.router = new GameClientMessageRouter() {
                Delegate = new RouterDelegate(this)
            };
        }

        public void Connect(string host, int port) {
            this.networkingClient.Connect(host, port);
        }

        public void Update() {
            this.router.Route(this.networkingClient.Read());
            this.networkingClient.Flush();
        }

        internal class GameClientDelegate {
            private WeakReference weakGameClient;

            protected GameClient gameClient { get { return this.weakGameClient?.Target as GameClient; } }

            public GameClientDelegate(GameClient client) {
                this.weakGameClient = new WeakReference(client);
            }
        }

        class ConnectorDelegate: GameClientDelegate, IGameClientConnectorDelegate {
            public ConnectorDelegate(GameClient client) : base(client) { }

            void IGameClientConnectorDelegate.GameClientDidConnect() {
                this.gameClient.Delegate?.GameClientDidConnect();
            }

            void IGameClientConnectorDelegate.GameClientConnectDidTimeout() {
                this.gameClient.Delegate?.GameClientConnectDidTimeout();
            }

            void IGameClientConnectorDelegate.GameClientDidDisconnect() {
                this.gameClient.Delegate?.GameClientDidDisconnect();
            }
        }

        class RouterDelegate: GameClientDelegate, IGameClientMessageRouterDelegate {
            public RouterDelegate(GameClient client) : base(client) { }

            void IGameClientMessageRouterDelegate.StartGame() {

            }

            void IGameClientMessageRouterDelegate.SpawnPlayer(Messages.SpawnMessage message) {

            }

            void IGameClientMessageRouterDelegate.MirrorPlayerInfo(Messages.PlayerMirrorInfo message) {

            }

            void IGameClientMessageRouterDelegate.SyncPlayer(Messages.SyncMessage message) {

            }

            void IGameClientMessageRouterDelegate.CustomServerMessage(MessageContainer container) {

            }
        }
    }
}
