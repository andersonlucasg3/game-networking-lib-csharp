using System;

namespace GameNetworking {
    using Networking;
    using Commons;

    internal class GameClientConnector: BaseWorker<GameClient>, INetworkingClientDelegate {
        internal GameClientConnector(GameClient client) : base(client) {
            client.networkingClient.Delegate = this;
        }        

        internal void Connect(string host, int port) {
            this.Instance?.networkingClient.Connect(host, port);
        }

        internal void Disconnect() {
            this.Instance?.networkingClient.Disconnect();
        }

        #region INetworkingClientDelegate

        void INetworkingClientDelegate.NetworkingClientDidConnect() {
            this.Instance?.Delegate?.GameClientDidConnect();
        }

        void INetworkingClientDelegate.NetworkingClientConnectDidTimeout() {
            this.Instance?.Delegate?.GameClientConnectDidTimeout();
        }

        void INetworkingClientDelegate.NetworkingClientDidDisconnect() {
            this.Instance?.Delegate?.GameClientDidDisconnect();
        }

        #endregion
    }
}