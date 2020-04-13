using System;

namespace Networking.Models {
    using Commons.Models;
    using Commons.IO;
    using Sockets;

    public interface IReliableNetClient<TSocket, TDerived> : INetClient<TSocket, TDerived>
        where TDerived : IReliableNetClient<TSocket, TDerived>
        where TSocket : ITCPSocket {
        bool isConnected { get; }

        IReader reader { get; }
        IWriter writer { get; }

        void Connect(NetEndPoint endPoint, Action connectAction);
        void Disconnect(Action disconnectAction);
    }

    public class ReliableNetClient : NetClient<ITCPSocket, ReliableNetClient>, IReliableNetClient<ITCPSocket, ReliableNetClient> {
        public IReader reader { get; }
        public IWriter writer { get; }

        public ReliableNetClient(ITCPSocket socket, IReader reader, IWriter writer) : base(socket) {
            this.reader = reader;
            this.writer = writer;

            this.reader.listener = this;
        }

        public bool isConnected { get { return this.socket.isConnected; } }

        public void Connect(NetEndPoint endPoint, Action connectAction) {
            this.socket.Connect(endPoint, () => connectAction?.Invoke());
        }

        public void Disconnect(Action disconnectAction) {
            this.socket.Disconnect(disconnectAction);
        }

        public override void Read() {
            this.reader.Read();
        }

        public override void Write(byte[] bytes) {
            this.writer.Write(bytes);
        }
    }
}