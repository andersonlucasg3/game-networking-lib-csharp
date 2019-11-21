using System;
using Networking;
using Google.Protobuf;
using Commons;

namespace MatchMaking.Connection {
    using Models;
    using Networking.Models;
    using Protobuf.Coders;

    public sealed class ClientConnection<MMClient>: WeakDelegate<IClientConnectionDelegate<MMClient>>, INetworkingDelegate, INetClientReadDelegate where MMClient: Client, new() {
        private readonly INetworking networking;

        private MMClient client;
        
        public bool IsConnecting { get; private set; }

        public bool IsConnected { get { return this.client?.IsConnected ?? false; } }

        public ClientConnection(INetworking networking) {
            this.networking = networking;
        }

        public void Connect(string host, int port) {
            this.IsConnecting = true;
            this.networking.Connect(host, port);
        }

        public void Read() {
            if (this.client?.client != null) {
                this.networking.Read(this.client.client);
            }
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

        #region INetClientReadDelegate

        void INetClientReadDelegate.ClientDidReadBytes(NetClient client, byte[] bytes) {
            this.client.decoder.Add(bytes);
            MessageContainer message = null;
            do {
                message = this.client.decoder.Decode();
                this.Delegate?.ClientDidReadMessage(message);
            } while (message != null);
        }

        #endregion

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
