using Networking;
using Google.Protobuf;

namespace MatchMaking {
    using Models;
    using Protobuf.Coders;

    public sealed class MatchMakingConnection<MMClient> where MMClient: MatchMakingClient, new() {
        private INetworking networking;

        private MMClient client;

        public bool IsConnected { get { return this.client.IsConnected; } }

        public MatchMakingConnection(INetworking networking) {
            this.networking = networking;
        }

        public void Connect(string host, int port) {
            this.client = MatchMakingClient.Create<MMClient>(
                this.networking.Connect(host, port),
                new MessageDecoder(),
                new MessageEncoder()
            );
        }

        public MessageContainer Read() {
            byte[] bytes = this.networking.Read(this.client.client);
            this.client.decoder.Add(bytes);
            return this.client.decoder.Decode();
        }

        public void Send<Message>(Message message) where Message: IMessage {
            byte[] bytes = this.client.encoder.Encode(message);
            this.networking.Send(this.client.client, bytes);
        }

        public void Disconnect() {
            this.networking.Disconnect(this.client.client);
        }
    }
}