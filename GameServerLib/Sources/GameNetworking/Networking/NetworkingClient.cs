using Networking;
using Networking.Models;
using Messages.Models;
using Messages.Streams;

namespace GameNetworking.Networking {
    using Models;

    internal class NetworkingClient : INetworkingListener, INetClientReadListener {
        private INetworking networking;
        private NetworkClient client;

        internal INetworkingClientListener listener { get; set; }

        public NetworkingClient(INetworking backend) {
            this.networking = backend;
            this.networking.listener = this;
        }

        public void Connect(string host, int port) {
            this.networking.Connect(host, port);
        }

        public void Disconnect() {
            if (this.client?.client != null) {
                this.networking.Disconnect(this.client.client);
            }
        }

        public void Send(ITypedMessage message) {
            this.client?.Write(message);
        }

        public void Update() {
            if (this.client?.client == null) { return; }
            this.networking.Read(this.client.client);
            this.networking.Flush(this.client.client);
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
