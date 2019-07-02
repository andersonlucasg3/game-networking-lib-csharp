using Networking;
using Networking.Models;
using Messages.Coders;
using Messages.Models;
using Messages.Streams;
using System;

namespace GameNetworking.Networking {
    using Models;

    internal class NetworkingClient: INetworkingDelegate {
        private INetworking networking;

        private NetworkClient client;

        private WeakReference weakDelegate;

        public INetworkingClientDelegate Delegate {
            get { return this.weakDelegate?.Target as INetworkingClientDelegate; }
            set { this.weakDelegate = new WeakReference(value); }
        }

        public NetworkingClient() {
            this.networking = new NetSocket();
            this.networking.Delegate = this;
        }

        public void Connect(string host, int port) {
            this.networking.Connect(host, port);
        }

        public void Disconnect() {
            if (this.client?.Client != null) {
                this.networking.Disconnect(this.client.Client);
            }
        }

        public MessageContainer Read() {
            if (this.client?.Client != null) {
                byte[] bytes = this.networking.Read(this.client.Client);
                this.client.Reader.Add(bytes);
                return this.client.Reader.Decode();
            }
            return null;
        }

        public void Send(ITypedMessage message) {
            this.client?.Write(message);
        }

        public void Flush() {
            if (this.client?.Client != null) {
                this.networking.Flush(this.client.Client);
            }
        }

        #region INetworkingDelegate

        void INetworkingDelegate.NetworkingDidConnect(NetClient client) {
            this.client = new NetworkClient(client, new MessageStreamReader(), new MessageStreamWriter());
            this.Delegate?.NetworkingClientDidConnect();
        }

        void INetworkingDelegate.NetworkingConnectDidTimeout() {
            this.client = null;
            this.Delegate?.NetworkingClientConnectDidTimeout();
        }

        void INetworkingDelegate.NetworkingDidDisconnect(NetClient client) {
            this.client = null;
            this.Delegate?.NetworkingClientDidDisconnect();
        }

        #endregion
    }
}
