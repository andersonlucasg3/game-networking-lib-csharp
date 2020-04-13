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

        IListener listener { get; set; }

        void Close();

        void Read();
        void Write(byte[] bytes);
    }

    public abstract class NetClient<TSocket, TDerived> : INetClient<TSocket, TDerived>, IReader.IListener where TSocket : ISocket where TDerived : NetClient<TSocket, TDerived> {
        TSocket INetClient<TSocket, TDerived>.socket { get; set; }

        private INetClient<TSocket, TDerived> self => this;

        protected TSocket socket { get => self.socket; private set => self.socket = value; }

        public INetClient<TSocket, TDerived>.IListener listener { get; set; }

        internal NetClient(TSocket socket) {
            this.socket = socket;
        }

        public abstract void Close();

        public abstract void Read();
        public abstract void Write(byte[] bytes);

        public override bool Equals(object obj) {
            if (obj is TSocket client) {
                return this.Equals(client);
            }
            if (obj is Socket socket) {
                return this.socket.Equals(socket);
            }
            if (obj is TDerived derived) {
                return this.socket.Equals(derived.socket);
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