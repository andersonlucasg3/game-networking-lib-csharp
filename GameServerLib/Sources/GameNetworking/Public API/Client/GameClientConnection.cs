using Messages.Models;

namespace GameNetworking {
    using Networking;
    using Commons;

    internal class GameClientConnection : BaseWorker<GameClient>, INetworkingClientListener {
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

        void INetworkingClientListener.NetworkingClientDidConnect() {
            this.instance?.listener?.GameClientDidConnect();
        }

        void INetworkingClientListener.NetworkingClientConnectDidTimeout() {
            this.instance?.listener?.GameClientConnectDidTimeout();
        }

        void INetworkingClientListener.NetworkingClientDidDisconnect() {
            this.instance?.listener?.GameClientDidDisconnect();
        }

        void INetworkingClientListener.NetworkingClientDidReadMessage(MessageContainer container) {
            this.instance?.GameClientConnectionDidReceiveMessage(container);
        }

        #endregion
    }
}