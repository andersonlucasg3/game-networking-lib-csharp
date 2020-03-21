using Networking.Commons.IO;
using Networking.Commons.Models;
using Networking.Sockets;

namespace Networking.Models {
    public class UnreliableNetClient : NetClient<IUDPSocket, UnreliableNetClient> {
        public UnreliableNetClient(IUDPSocket socket, IReader reader, IWriter writer) : base(socket, reader, writer) { }
    }
}
