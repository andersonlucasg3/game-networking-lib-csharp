using Networking.Commons.IO;
using Networking.Commons.Models;
using Networking.IO;
using Networking.Sockets;

namespace Networking.Models {
    public interface IUnreliableNetClient<TSocket, TDerived> : INetClient<TSocket, TDerived>
        where TDerived : IUnreliableNetClient<TSocket, TDerived>
        where TSocket : IUDPSocket {
        bool isCommunicable { get; }
    }

    public class UnreliableNetClient : NetClient<IUDPSocket, UnreliableNetClient>, IUnreliableNetClient<IUDPSocket, UnreliableNetClient>, UnreliableNetworkingReader.IListener {
        public bool isCommunicable => this.socket.isCommunicable;
        
        public UnreliableNetClient(IUDPSocket socket) : base(socket) { }

        public override void Read() {
            // TODO: what am I gone do here?
        }

        public override void Write(byte[] bytes) {
            // TODO: what am I gone do here?
        }

        public void ReaderDidRead(byte[] bytes, UDPSocket from) {
            // TODO: what am I gone do here?
        }
    }
}
