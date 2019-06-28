namespace MatchMaking.Models {
    using Coders;

    public class Client {
        internal Networking.Models.NetClient client;
        internal IMessageDecoder decoder;
        internal IMessageEncoder encoder;

        public bool IsConnected { get { return this.client.IsConnected; } }

        internal static MMClient Create<MMClient>(Networking.Models.NetClient client, IMessageDecoder decoder, IMessageEncoder encoder) where MMClient: Client, new() {
            return new MMClient {
                client = client,
                decoder = decoder,
                encoder = encoder
            };
        }

        protected Client() { }

        public override bool Equals(object obj) {
            if (obj is Client) {
                Client other = obj as Client;
                return other.client == this.client;
            }
            return object.ReferenceEquals(this, obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}