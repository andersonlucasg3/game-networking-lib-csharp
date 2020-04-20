using Networking.Commons.Models;
using Networking.IO;
using Networking.Sockets;

namespace Networking.Models {
    public interface IUnreliableNetClient<TSocket, TDerived> : INetClient<TSocket, TDerived>
        where TDerived : IUnreliableNetClient<TSocket, TDerived>
        where TSocket : IUDPSocket {
        bool isCommunicable { get; }
    }

    public class UnreliableNetClient : NetClient<IUDPSocket, UnreliableNetClient>, IUnreliableNetClient<IUDPSocket, UnreliableNetClient> {
        private readonly UnreliableNetworkingWriter writer;

        public bool isCommunicable => this.socket.isCommunicable;

        public UnreliableNetClient(IUDPSocket socket) : base(socket) {
            this.writer = new UnreliableNetworkingWriter(socket);
        }

        public override void Close() {
            this.socket.Unbind();
        }

        public override void Read() {
            // TODO: I think I should do nothing here...
        }

        public override void Write(byte[] bytes) {
            this.writer.Write(bytes);
        }

        public void Flush() {
            this.writer.Flush();
        }
    }
}
