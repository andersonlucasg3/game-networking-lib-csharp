using System;

namespace GameNetworking {
    using Networking;

    internal class GameClientConnector: BaseClientWorker, INetworkingClientDelegate {
        internal GameClientConnector(GameClient client) : base(client) {
            client.networkingClient.Delegate = this;
        }        

        internal void Connect(string host, int port) {
            this.Client?.networkingClient.Connect(host, port);
        }

        internal void Disconnect() {
            this.Client?.networkingClient.Disconnect();
        }

        #region INetworkingClientDelegate

        void INetworkingClientDelegate.NetworkingClientDidConnect() {
            this.Client?.Delegate?.GameClientDidConnect();
        }

        void INetworkingClientDelegate.NetworkingClientConnectDidTimeout() {
            this.Client?.Delegate?.GameClientConnectDidTimeout();
        }

        void INetworkingClientDelegate.NetworkingClientDidDisconnect() {
            this.Client?.Delegate?.GameClientDidDisconnect();
        }

        #endregion
    }
}