using Messages.Streams;
using Messages.Models;
using System;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace GameNetworking.Commons.Models {
    public interface INetworkClient<TSocket, TNetClient> : IEquatable<INetworkClient<TSocket, TNetClient>>
        where TSocket : ISocket
        where TNetClient : INetClient<TSocket, TNetClient> {
        TNetClient client { get; }
        IStreamReader reader { get; }
        IStreamWriter writer { get; }

        void Write<TMessage>(TMessage message) where TMessage : ITypedMessage;
    }

    public abstract class NetworkClient<TSocket, TNetClient> : INetworkClient<TSocket, TNetClient> where TSocket : ISocket where TNetClient : INetClient<TSocket, TNetClient> {
        public TNetClient client { get; private set; }

        public IStreamReader reader { get; private set; }
        public IStreamWriter writer { get; private set; }

        public NetworkClient(TNetClient client, IStreamReader reader, IStreamWriter writer) {
            this.client = client;
            this.reader = reader;
            this.writer = writer;
        }

        public void Write<TMessage>(TMessage message) where TMessage : ITypedMessage {
            if (!this.client.socket.isCommunicable) { return; }

            this.client.Write(this.writer.Write(message));
        }

        public override bool Equals(object obj) {
            if (obj is INetworkClient<TSocket, TNetClient> n_client) {
                return this.Equals(n_client);
            }
            if (obj is INetClient<TSocket, TNetClient> client) {
                return this.client.Equals(client);
            }
            return object.Equals(this, obj);
        }

        public bool Equals(INetworkClient<TSocket, TNetClient> other) {
            var selfOther = (NetworkClient<TSocket, TNetClient>)other;
            return this.client.Equals(selfOther.client);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}