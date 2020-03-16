using System.Net.Sockets;

namespace Networking.Models {
    using IO;
    using System;

    public sealed class NetClient : INetClient, IEquatable<NetClient>, IReaderListener {
        internal ISocket socket;

        public IReader reader { get; }
        public IWriter writer { get; }

        public bool isConnected { get { return this.socket.isConnected; } }

        public INetClientReadListener listener { get; set; }

        internal NetClient(ISocket socket, IReader reader, IWriter writer) {
            this.socket = socket;
            this.reader = reader;
            this.writer = writer;

            this.reader.listener = this;
        }

        public void Connect(NetEndPoint endPoint, Action connectAction) {
            this.socket.Connect(endPoint, () => connectAction?.Invoke());
        }

        public void Disconnect(Action disconnectAction) {
            this.socket.Disconnect(disconnectAction);
        }

        public override bool Equals(object obj) {
            if (obj is NetClient n_client) {
                return this.Equals(n_client);
            }
            if (obj is Socket socket) {
                return this.socket.Equals(socket);
            }
            return object.Equals(this, obj);
        }

        public bool Equals(NetClient client) {
            return this.socket.Equals(client.socket);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        void IReaderListener.ClientDidRead(byte[] bytes) {
            listener?.ClientDidReadBytes(this, bytes);
        }
    }
}
