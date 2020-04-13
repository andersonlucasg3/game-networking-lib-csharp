using GameNetworking.Commons;
using GameNetworking.Commons.Client;
using GameNetworking.Commons.Models.Client;
using GameNetworking.Messages.Client;
using GameNetworking.Networking;
using GameNetworking.Networking.Models;
using Networking.Models;
using Networking.Sockets;
using GameNetworking.Networking.Commons;
using Messages.Models;
using System;

namespace GameNetworking {
    public class UnreliableGameClient<TPlayer> : GameClient<UnreliableNetworkingClient, TPlayer, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient, UnreliableGameClient<TPlayer>>, INetworkingClient<IUDPSocket, UnreliableNetworkClient, UnreliableNetClient>.IListener
        where TPlayer : class, INetworkPlayer<IUDPSocket, UnreliableNetworkClient, UnreliableNetClient>, new() {

        internal readonly UnreliableClientConnectionController clientConnectionController;

        public double timeOutDelay { get; set; } = 10F;

        public UnreliableGameClient(UnreliableNetworkingClient backend, IMainThreadDispatcher dispatcher) : base(backend, new UnreliableClientMessageRouter<TPlayer>(dispatcher)) {
            this.networkingClient.listener = this;

            this.clientConnectionController = new UnreliableClientConnectionController(this, this.ConnectionDidTimeOut);
        }

        public void Start(string host, int port) {
            this.networkingClient.Start(host, port);
        }

        public override void Connect(string host, int port) {
            this.networkingClient.Connect(host, port);

            this.clientConnectionController.Connect();
        }

        public override void Disconnect() {
            this.Send(new UnreliableDisconnectMessage());
            this.Send(new UnreliableDisconnectMessage());
            this.Send(new UnreliableDisconnectMessage());
        }

        public override void Update() {
            base.Update();

            this.clientConnectionController.Update();

            if (this.localPlayer == null) { return; }

            var now = TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;
            var elapsedTime = this.localPlayer.lastReceivedPingRequest - now;
            if (elapsedTime >= this.timeOutDelay) {
                this.Disconnect();
                this.DidDisconnect();
            }
        }

        internal void DidConnect() {
            this.clientConnectionController.ReceivedConnected();
            this.listener?.GameClientDidConnect();
        }

        internal void DidDisconnect() {
            this.listener?.GameClientDidDisconnect();
        }

        void INetworkingClient<IUDPSocket, UnreliableNetworkClient, UnreliableNetClient>.IListener.NetworkingClientDidReadMessage(MessageContainer container) {
            this.GameClientConnectionDidReceiveMessage(container);
        }

        #region Private Methods

        private void ConnectionDidTimeOut() {
            this.listener?.GameClientConnectDidTimeout();
        }

        #endregion
    }
}