using Networking.Models;
using Messages.Streams;
using Messages.Models;
using System;

namespace GameNetworking.Models {

    public sealed class NetworkClient : IEquatable<NetworkClient> {
        internal INetClient client { get; private set; }
        internal IStreamReader Reader { get; private set; }
        internal IStreamWriter Writer { get; private set; }

        internal NetworkClient(INetClient client, IStreamReader reader, IStreamWriter writer) {
            this.client = client;
            this.Reader = reader;
            this.Writer = writer;
        }

        internal void Write<Message>(Message message) where Message : ITypedMessage {
            this.client.writer.Write(this.Writer.Write(message));
        }

        public override bool Equals(object obj) {
            if (obj is NetworkClient client) {
                return this.Equals(client);
            }
            if (obj is INetClient n_client) {
                return this.client.Equals(n_client);
            }
            return object.Equals(this, obj);
        }

        public bool Equals(NetworkClient other) {
            return this.client.Equals(other.client);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}