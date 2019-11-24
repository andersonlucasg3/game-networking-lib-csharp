using Messages.Models;

namespace GameNetworking {
    using Networking;
    using Commons;

    internal class GameClientConnection : BaseWorker<GameClient>, INetworkingClientDelegate {
        internal GameClientConnection(GameClient client) : base(client) {
            client.networkingClient.listener = this;
        }

        internal void Connect(string host, int port) {
            this.Instance?.networkingClient.Connect(host, port);
        }

        internal void Disconnect() {
            this.Instance?.networkingClient.Disconnect();
        }

        #region INetworkingClientDelegate

        void INetworkingClientDelegate.NetworkingClientDidConnect() {
            this.Instance?.listener?.GameClientDidConnect();
        }

        void INetworkingClientDelegate.NetworkingClientConnectDidTimeout() {
            this.Instance?.listener?.GameClientConnectDidTimeout();
        }

        void INetworkingClientDelegate.NetworkingClientDidDisconnect() {
            this.Instance?.listener?.GameClientDidDisconnect();
        }

        void INetworkingClientDelegate.NetworkingClientDidReadMessage(MessageContainer container) {
            this.Instance?.GameClientConnectionDidReceiveMessage(container);
        }

        #endregion
    }
}