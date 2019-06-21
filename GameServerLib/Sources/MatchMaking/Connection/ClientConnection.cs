using Networking;
using Google.Protobuf;

namespace MatchMaking.Connection {
    using Models;
    using Protobuf.Coders;

    public sealed class ClientConnection<MMClient> where MMClient: Client, new() {
        private INetworking networking;

        private MMClient client;

        public bool IsConnected { get { return this.client?.IsConnected ?? false; } }

        public ClientConnection(INetworking networking) {
            this.networking = networking;
        }

        public void Connect(string host, int port) {
            this.client = Client.Create<MMClient>(this.networking.Connect(host, port),
                new MessageDecoder(), new MessageEncoder());
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
            byte[] bytes;
            if ((bytes = this.client?.encoder?.Encode(message)) != null) {
                this.networking.Send(this.client.client, bytes);
            }
        }

        public void Disconnect() {
            this.networking.Disconnect(this.client.client);
        }
    }
}
