using GameNetworking.Commons;
using GameNetworking.Commons.Client;
using GameNetworking.Commons.Models.Client;
using GameNetworking.Messages.Models;
using GameNetworking.Networking;
using GameNetworking.Networking.Commons;
using GameNetworking.Networking.Models;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking {
    public class UnreliableGameClient<TPlayer> : GameClient<UnreliableNetworkingClient, TPlayer, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient, UnreliableGameClient<TPlayer>>, IUnreliableNetworkingClientListener
        where TPlayer : class, INetworkPlayer<IUDPSocket, UnreliableNetworkClient, UnreliableNetClient>, new() {

        public UnreliableGameClient(UnreliableNetworkingClient backend, IMainThreadDispatcher dispatcher) : base(backend, new UnreliableClientMessageRouter<TPlayer>(dispatcher)) {
            this.networkingClient.listener = this;
        }

        public void Start(string host, int port) {
            this.networkingClient.Start(host, port);
        }

        public override void Connect(string host, int port) {
            this.networkingClient.Connect(host, port);
        }

        public override void Disconnect() {
            this.networkingClient.Disconnect();
        }

        internal override void DidDisconnect() {
            if (this.localPlayer != null) {
                this.playersStorage.Clear();
                this.networkingClient.Close();
            }
            base.DidDisconnect();
        }

        void INetworkingClientListener.NetworkingClientDidReadMessage(MessageContainer container) {
            this.GameClientConnectionDidReceiveMessage(container);
        }

        void IUnreliableNetworkingClientListener.UnreliableNetworkingClientDidConnect() {
            this.listener?.GameClientDidConnect();
        }

        void IUnreliableNetworkingClientListener.UnreliableNetworkingClientConnectDidTimeout() {
            this.listener?.GameClientConnectDidTimeout();
        }

        void IUnreliableNetworkingClientListener.UnreliableNetworkingClientDidDisconnect() {
            this.DidDisconnect();
            this.listener?.GameClientDidDisconnect();
        }
    }
}