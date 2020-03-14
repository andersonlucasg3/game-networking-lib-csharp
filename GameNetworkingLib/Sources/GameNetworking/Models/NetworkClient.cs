using Networking.Models;
using Messages.Streams;
using Messages.Models;

namespace GameNetworking.Models {
    
    public sealed class NetworkClient {
        internal INetClient client { get; private set; }
        internal IStreamReader Reader { get; private set; }
        internal IStreamWriter Writer { get; private set; }

        internal NetworkClient(INetClient client, IStreamReader reader, IStreamWriter writer) {
            this.client = client;
            this.Reader = reader;
            this.Writer = writer;
        }

        internal void Write<Message>(Message message) where Message: ITypedMessage {
            this.client.writer.Write(this.Writer.Write(message));
        }

        public override bool Equals(object obj) {
            if (obj is NetworkClient) {
                return this.client == ((NetworkClient)obj).client;
            }
            if (obj is INetClient) {
                return this.client == obj;
            }
            return object.Equals(this, obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}