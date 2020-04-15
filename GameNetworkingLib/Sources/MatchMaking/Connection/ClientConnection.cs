#if ENABLE

using Networking;
using Google.Protobuf;
using Networking.Commons.Models;
using Networking.Models;
using Networking.Sockets;

namespace MatchMaking.Connection {
    using Models;
    using Protobuf.Coders;

    public sealed class ClientConnection<TClient>: IReliableSocket.IListener, INetClient<ITCPSocket, ReliableNetClient>.IListener where TClient: MatchMakingClient, new() {
        private readonly IReliableSocket networking;

        private TClient client;
        
        public bool IsConnecting { get; private set; }

        public bool IsConnected { get { return this.client?.IsConnected ?? false; } }

        public IClientConnectionDelegate<TClient> listener { get; set; }

        public ClientConnection(IReliableSocket networking) {
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

        public void Send<TMessage>(TMessage message) where TMessage: IMessage {
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

        #region INetClient<ITCPSocket>.IListener

        void INetClient<ITCPSocket, ReliableNetClient>.IListener.ClientDidReadBytes(ReliableNetClient client, byte[] bytes) {
            this.client.decoder.Add(bytes);
            MessageContainer message;
            do {
                message = this.client.decoder.Decode();
                this.listener?.ClientDidReadMessage(message);
            } while (message != null);
        }

        #endregion

        #region IReliableNetworking.IListener

        void IReliableSocket.IListener.NetworkingDidConnect(ReliableNetClient client) {
            this.client = MatchMakingClient.Create<TClient>(client, new MessageDecoder(), new MessageEncoder());
            this.IsConnecting = false;
            this.listener?.ClientConnectionDidConnect();
        }

        void IReliableSocket.IListener.NetworkingConnectDidTimeout() {
            this.client = null;
            this.IsConnecting = false;
            this.listener?.ClientConnectionDidTimeout();
        }

        void IReliableSocket.IListener.NetworkingDidDisconnect(ReliableNetClient client) {
            this.client = null;
            this.IsConnecting = false;
            this.listener?.ClientConnectionDidDisconnect();
        }

        #endregion
    }
}

#endif