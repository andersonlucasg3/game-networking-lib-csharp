using System;

namespace GameNetworking {
    using Networking;

    internal interface IGameClientConnectorDelegate {
        void GameClientDidConnect();
        void GameClientConnectDidTimeout();
        void GameClientDidDisconnect();
    }

    internal class GameClientConnector: INetworkingClientDelegate {
        private readonly WeakReference weakNetowrking;

        //private WeakReference weakDelegate;

        private NetworkingClient Networking {  get { return this.weakNetowrking?.Target as NetworkingClient; } }

        internal IGameClientConnectorDelegate Delegate {
            get;// { return this.weakDelegate?.Target as IGameClientConnectorDelegate; }
            set;// { this.weakDelegate = new WeakReference(value); }
        }

        internal GameClientConnector(NetworkingClient networking) {
            this.weakNetowrking = new WeakReference(networking);
            networking.Delegate = this;
        }

        internal void Connect(string host, int port) {
            this.Networking.Connect(host, port);
        }

        internal void Disconnect() {
            this.Networking.Disconnect();
        }

        #region INetworkingClientDelegate

        void INetworkingClientDelegate.NetworkingClientDidConnect() {
            this.Delegate?.GameClientDidConnect();
        }

        void INetworkingClientDelegate.NetworkingClientConnectDidTimeout() {
            this.Delegate?.GameClientConnectDidTimeout();
        }

        void INetworkingClientDelegate.NetworkingClientDidDisconnect() {
            this.Delegate?.GameClientDidDisconnect();
        }

        #endregion
    }
}