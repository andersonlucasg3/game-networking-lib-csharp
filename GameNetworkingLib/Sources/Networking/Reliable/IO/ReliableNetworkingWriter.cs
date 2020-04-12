using Networking.Commons.IO;
using Networking.Sockets;

namespace Networking.IO {
    public class ReliableNetworkingWriter : NetworkingWriter<ITCPSocket> {
        public ReliableNetworkingWriter(ITCPSocket socket) : base(socket) { }
    }
}