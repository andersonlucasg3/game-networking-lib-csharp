using Networking.Models;
using Messages.Streams;

namespace GameNetworking.Models {
    public sealed class NetworkClient {
        internal Client Client { get; private set; }
        internal IStreamReader Reader { get; private set; }
        internal IStreamWriter Writer { get; private set; }

        internal NetworkClient(Client client, IStreamReader reader, IStreamWriter writer) {
            this.Client = client;
            this.Reader = reader;
            this.Writer = writer;
        }

        public override bool Equals(object obj) {
            if (obj is NetworkClient) {
                return this.Client == ((NetworkClient)obj).Client;
            } else if (obj is Client) {
                return this.Client == obj;
            }
            return object.Equals(this, obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}