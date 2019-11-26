using Messages.Models;

namespace GameNetworking {
    using Networking;
    using Commons;

    internal class GameClientConnection : BaseWorker<GameClient>, INetworkingClientDelegate {
        internal GameClientConnection(GameClient client) : base(client) {
            client.networkingClient.listener = this;
        }

        internal void Connect(string host, int port) {
            this.instance?.networkingClient.Connect(host, port);
        }

        internal void Disconnect() {
            this.instance?.networkingClient.Disconnect();
        }

        #region INetworkingClientDelegate

        void INetworkingClientDelegate.NetworkingClientDidConnect() {
            this.instance?.listener?.GameClientDidConnect();
        }

        void INetworkingClientDelegate.NetworkingClientConnectDidTimeout() {
            this.instance?.listener?.GameClientConnectDidTimeout();
        }

        void INetworkingClientDelegate.NetworkingClientDidDisconnect() {
            this.instance?.listener?.GameClientDidDisconnect();
        }

        void INetworkingClientDelegate.NetworkingClientDidReadMessage(MessageContainer container) {
            this.instance?.GameClientConnectionDidReceiveMessage(container);
        }

        #endregion
    }
}