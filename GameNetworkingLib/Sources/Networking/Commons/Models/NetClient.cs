using System;
using System.Net.Sockets;

namespace Networking.Commons.Models {
    using IO;
    using Sockets;

    public interface INetClient<TSocket, TDerived> : IEquatable<TDerived> where TDerived : INetClient<TSocket, TDerived> {
        public interface IListener {
            void ClientDidReadBytes(TDerived client, byte[] bytes);
        }

        TSocket socket { get; internal set; }

        IReader reader { get; }
        IWriter writer { get; }

        IListener listener { get; set; }
    }

    public abstract class NetClient<TSocket, TDerived> : INetClient<TSocket, TDerived>, IReader.IListener where TSocket : ISocket where TDerived : NetClient<TSocket, TDerived> {
        TSocket INetClient<TSocket, TDerived>.socket { get; set; }

        private INetClient<TSocket, TDerived> self => this;

        protected TSocket socket { get => self.socket; private set => self.socket = value; }

        public IReader reader { get; }
        public IWriter writer { get; }

        public INetClient<TSocket, TDerived>.IListener listener { get; set; }

        internal NetClient(TSocket socket, IReader reader, IWriter writer) {
            this.socket = socket;
            this.reader = reader;
            this.writer = writer;

            this.reader.listener = this;
        }

        public override bool Equals(object obj) {
            if (obj is TSocket client) {
                return this.Equals(client);
            }
            if (obj is Socket socket) {
                return this.socket.Equals(socket);
            }
            return object.Equals(this, obj);
        }

        public bool Equals(TDerived client) {
            return this.socket.Equals(client.socket);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        void IReader.IListener.ClientDidRead(byte[] bytes) {
            listener?.ClientDidReadBytes(this as TDerived, bytes);
        }
    }
}