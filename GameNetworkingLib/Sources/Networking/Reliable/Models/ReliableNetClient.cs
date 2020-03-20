using System;

namespace Networking.Models {
    using Commons.Models;
    using Commons.IO;
    using Sockets;

    public interface IReliableNetClient<TDerived> : INetClient<ITCPSocket, TDerived> where TDerived : IReliableNetClient<TDerived> {
        bool isConnected { get; }

        void Connect(NetEndPoint endPoint, Action connectAction);
        void Disconnect(Action disconnectAction);
    }

    public class ReliableNetClient : NetClient<ITCPSocket, ReliableNetClient>, IReliableNetClient<ReliableNetClient> {
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
