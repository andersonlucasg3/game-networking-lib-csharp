using System;
using Networking;
using Google.Protobuf;

namespace MatchMaking.Connection {
    using Models;
    using Protobuf.Coders;

    public sealed class ClientConnection<MMClient>: INetworkingDelegate where MMClient: Client, new() {
        private readonly INetworking networking;

        private MMClient client;
        private WeakReference weakDelegate;

        public bool IsConnecting { get; private set; }

        public bool IsConnected { get { return this.client?.IsConnected ?? false; } }

        public IClientConnectionDelegate<MMClient> Delegate {
            get { return this.weakDelegate?.Target as IClientConnectionDelegate<MMClient>; }
            set { this.weakDelegate = new WeakReference(value); }
        }

        public ClientConnection(INetworking networking) {
            this.networking = networking;
        }

        public void Connect(string host, int port) {
            this.IsConnecting = true;
            this.networking.Connect(host, port);
        }

        public MessageContainer Read() {
            if (this.client?.client != null) {
                byte[] bytes = this.networking.Read(this.client.client);
                this.client.decoder.Add(bytes);
                return this.client.decoder.Decode();
            }
            return null;
        }

        public void Send<Message>(Message message) where Message: IMessage {
            byte[] bytes = this.client?.encoder?.Encode(message);
            if (bytes != null) {
                this.networking.Send(this.client.client, bytes);
            }
        }

        public void Flush() {
            if (this.client?.client != null) {
                this.networking.Flush(this.client.client);
            }
        }

        public void Disconnect() {
            if (this.client?.client != null) {
                this.networking?.Disconnect(this.client.client);
            }
        }

        #region INetworkingDelegate

        void INetworkingDelegate.NetworkingDidConnect(Networking.Models.NetClient client) {
            this.client = Client.Create<MMClient>(client, new MessageDecoder(), new MessageEncoder());
            this.IsConnecting = false;
            this.Delegate?.ClientConnectionDidConnect();
        }

        void INetworkingDelegate.NetworkingConnectDidTimeout() {
            this.client = null;
            this.IsConnecting = false;
            this.Delegate?.ClientConnectionDidTimeout();
        }

        void INetworkingDelegate.NetworkingDidDisconnect(Networking.Models.NetClient client) {
            this.client = null;
            this.IsConnecting = false;
            this.Delegate?.ClientConnectionDidDisconnect();
        }

        #endregion
    }
}
