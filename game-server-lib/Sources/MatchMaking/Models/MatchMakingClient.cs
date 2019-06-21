namespace MatchMaking.Models {
    using Networking.Models;
    using Coders;

    public class MatchMakingClient {
        internal Client client;
        internal IMessageDecoder decoder;
        internal IMessageEncoder encoder;

        public bool IsConnected { get { return this.client.IsConnected; } }

        internal static MMClient Create<MMClient>(Client client, IMessageDecoder decoder, IMessageEncoder encoder) where MMClient: MatchMakingClient, new() {
            return new MMClient {
                client = client,
                decoder = decoder,
                encoder = encoder
            };
        }

        internal MatchMakingClient() { }

        public override bool Equals(object obj) {
            if (obj is MatchMakingClient) {
                MatchMakingClient other = obj as MatchMakingClient;
                return other.client == this.client;
            }
            return object.ReferenceEquals(this, obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}