using Networking;
using Google.Protobuf;

namespace MatchMaking.Connection {
    using Models;
    using Protobuf.Coders;

    public delegate void ClientConnectionDelegate();

    public sealed class ClientConnection<MMClient> where MMClient: Client, new() {
        private INetworking networking;

        private MMClient client;

        public bool IsConnected { get { return this.client?.IsConnected ?? false; } }

        public ClientConnection(INetworking networking) {
            this.networking = networking;
        }

        public void Connect(string host, int port, ClientConnectionDelegate connectionDelegate) {
            this.networking.Connect(host, port, (client) => {
                this.client = Client.Create<MMClient>(client,
                new MessageDecoder(), new MessageEncoder());
                connectionDelegate.Invoke();
            });
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
            this.networking.Disconnect(this.client.client);
        }
    }
}
