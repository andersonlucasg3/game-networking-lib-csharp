using GameNetworking.Commons.Models.Client;
using GameNetworking.Networking;
using GameNetworking.Networking.Commons;
using GameNetworking.Networking.Models;
using Messages.Models;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking {
    internal class GameClientConnection<TPlayer> : IReliableNetworkingClientListener
        where TPlayer : class, INetworkPlayer<ITCPSocket, ReliableNetworkClient, ReliableNetClient>, new() {
        private readonly ReliableGameClient<TPlayer> client;

        internal GameClientConnection(ReliableGameClient<TPlayer> client) {
            this.client = client;
            this.client.networkingClient.listener = this;
        }

        internal void Connect(string host, int port) {
            this.client?.networkingClient.Connect(host, port);
        }

        internal void Disconnect() {
            this.client?.networkingClient.Disconnect();
        }

        #region INetworkingClient<TSocket, TClient, TNetClient>.IListener

        void IReliableNetworkingClientListener.NetworkingClientDidConnect() {
            this.client?.listener?.GameClientDidConnect();
        }

        void IReliableNetworkingClientListener.NetworkingClientConnectDidTimeout() {
            this.client?.DidDisconnect();
            this.client?.listener?.GameClientConnectDidTimeout();
        }

        void IReliableNetworkingClientListener.NetworkingClientDidDisconnect() {
            this.client?.DidDisconnect();
            this.client?.listener?.GameClientDidDisconnect();
        }

        void INetworkingClientListener.NetworkingClientDidReadMessage(MessageContainer container) {
            this.client?.GameClientConnectionDidReceiveMessage(container);
        }

        #endregion
    }
}