using Networking.Models;
using Messages.Streams;
using Messages.Models;

namespace GameNetworking.Models {
    
    public sealed class NetworkClient {
        internal NetClient Client { get; private set; }
        internal IStreamReader Reader { get; private set; }
        internal IStreamWriter Writer { get; private set; }

        internal NetworkClient(NetClient client, IStreamReader reader, IStreamWriter writer) {
            this.Client = client;
            this.Reader = reader;
            this.Writer = writer;
        }

        internal void Write<Message>(Message message) where Message: ITypedMessage {
            this.Client.writer.Write(this.Writer.Write(message));
        }

        public override bool Equals(object obj) {
            if (obj is NetworkClient) {
                return this.Client == ((NetworkClient)obj).Client;
            } else if (obj is NetClient) {
                return this.Client == obj;
            }
            return object.Equals(this, obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}