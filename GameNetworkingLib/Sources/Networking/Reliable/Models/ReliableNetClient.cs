using System;

namespace Networking.Models {
    using Commons.Models;
    using Commons.IO;
    using Sockets;

    public interface IReliableNetClient<TSocket, TDerived> : INetClient<TSocket, TDerived>
        where TDerived : IReliableNetClient<TSocket, TDerived>
        where TSocket : ITCPSocket {
        bool isConnected { get; }

        void Connect(NetEndPoint endPoint, Action connectAction);
        void Disconnect(Action disconnectAction);
    }

    public class ReliableNetClient : NetClient<ITCPSocket, ReliableNetClient>, IReliableNetClient<ITCPSocket, ReliableNetClient> {
        public ReliableNetClient(ITCPSocket socket, IReader reader, IWriter writer) : base(socket, reader, writer) { }

        public bool isConnected { get { return this.socket.isConnected; } }

        public void Connect(NetEndPoint endPoint, Action connectAction) {
            this.socket.Connect(endPoint, () => connectAction?.Invoke());
        }

        public void Disconnect(Action disconnectAction) {
            this.socket.Disconnect(disconnectAction);
        }
    }
}
