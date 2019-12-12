using System.Net.Sockets;

namespace Networking.Models {
    using IO;
    using System;

    public sealed class NetClient : INetClient, IReaderListener {
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
            if (obj is NetClient) {
                return this.socket == ((NetClient)obj).socket;
            }
            if (obj is Socket) {
                return this.socket == obj;
            }
            return object.Equals(this, obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        void IReaderListener.ClientDidRead(byte[] bytes) {
            listener?.ClientDidReadBytes(this, bytes);
        }
    }
}
