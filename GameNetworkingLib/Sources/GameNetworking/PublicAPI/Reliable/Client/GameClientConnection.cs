using Messages.Models;
using GameNetworking.Networking.Commons;
using Networking.Sockets;
using GameNetworking.Commons.Models.Client;
using GameNetworking.Networking.Models;
using Networking.Models;
using GameNetworking.Networking;

namespace GameNetworking {
    internal class GameClientConnection<TPlayer> : ReliableNetworkingClient.IListener
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

        void ReliableNetworkingClient.IListener.NetworkingClientDidConnect() {
            this.client?.listener?.GameClientDidConnect();
        }

        void ReliableNetworkingClient.IListener.NetworkingClientConnectDidTimeout() {
            this.client?.listener?.GameClientConnectDidTimeout();
        }

        void ReliableNetworkingClient.IListener.NetworkingClientDidDisconnect() {
            this.client?.listener?.GameClientDidDisconnect();
        }

        void INetworkingClient<ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IListener.NetworkingClientDidReadMessage(MessageContainer container) {
            this.client?.GameClientConnectionDidReceiveMessage(container);
        }

        #endregion
    }
}