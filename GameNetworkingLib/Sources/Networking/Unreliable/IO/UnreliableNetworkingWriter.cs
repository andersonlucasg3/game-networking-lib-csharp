using Networking.Commons.IO;
using Networking.Sockets;

namespace Networking.IO {
    public class UnreliableNetworkingWriter : NetworkingWriter<IUDPSocket> {
        public UnreliableNetworkingWriter(IUDPSocket socket) : base(socket) { }
    }
}