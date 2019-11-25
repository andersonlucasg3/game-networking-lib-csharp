using Networking;
using Networking.Models;
using Messages.Models;
using Messages.Streams;
using Commons;

namespace GameNetworking.Networking {
    using Models;

    internal class NetworkingClient : WeakListener<INetworkingClientDelegate>, INetworkingListener, INetClientReadListener {
        private INetworking networking;
        private NetworkClient client;

        public NetworkingClient(INetworking backend) {
            this.networking = backend;
            this.networking.listener = this;
        }

        public void Connect(string host, int port) {
            this.networking.Connect(host, port);
        }

        public void Disconnect() {
            if (this.client?.Client != null) {
                this.networking.Disconnect(this.client.Client);
            }
        }

        public void Send(ITypedMessage message) {
            this.client?.Write(message);
        }

        public void Update() {
            if (this.client?.Client == null) { return; }
            this.networking.Read(this.client.Client);
            this.networking.Flush(this.client.Client);
        }

        #region INetClientReadDelegate

        void INetClientReadListener.ClientDidReadBytes(NetClient client, byte[] bytes) {
            this.client.Reader.Add(bytes);
            MessageContainer container;
            do {
                container = this.client.Reader.Decode();
                if (container == null) { continue; }
                this.listener?.NetworkingClientDidReadMessage(container);
            } while (container != null);
        }

        #endregion

        #region INetworkingDelegate

        void INetworkingListener.NetworkingDidConnect(INetClient client) {
            client.listener = this;

            this.client = new NetworkClient(client, new MessageStreamReader(), new MessageStreamWriter());
            this.listener?.NetworkingClientDidConnect();
        }

        void INetworkingListener.NetworkingConnectDidTimeout() {
            this.client = null;
            this.listener?.NetworkingClientConnectDidTimeout();
        }

        void INetworkingListener.NetworkingDidDisconnect(INetClient client) {
            this.client = null;
            this.listener?.NetworkingClientDidDisconnect();
        }

        #endregion
    }
}
