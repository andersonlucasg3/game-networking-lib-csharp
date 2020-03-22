using Networking.Commons.IO;
using Networking.Commons.Models;
using Networking.Sockets;

namespace Networking.Models {
    public interface IUnreliableNetClient<TSocket, TDerived> : INetClient<TSocket, TDerived>
        where TDerived : IUnreliableNetClient<TSocket, TDerived>
        where TSocket : IUDPSocket {
        bool isCommunicable { get; }
    }

    public class UnreliableNetClient : NetClient<IUDPSocket, UnreliableNetClient>, IUnreliableNetClient<IUDPSocket, UnreliableNetClient> {
        public bool isCommunicable => this.socket.isCommunicable;

        public UnreliableNetClient(IUDPSocket socket, IReader reader, IWriter writer) : base(socket, reader, writer) { }
    }
}
