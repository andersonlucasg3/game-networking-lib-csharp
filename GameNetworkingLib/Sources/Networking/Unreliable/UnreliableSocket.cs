using System.Net;
using Networking.Commons;
using Networking.Commons.Models;
using Networking.Models;

namespace Networking.Sockets {
    public class UnreliableSocket : INetworking<IUDPSocket, UnreliableNetClient> {
        private readonly IUDPSocket socket;

        public int port { get; private set; }

        public UnreliableSocket(IUDPSocket socket) {
            this.socket = socket;
        }

        public void Start(int port) {
            this.port = port;
            NetEndPoint ep = new NetEndPoint(IPAddress.Any.ToString(), port);
            this.socket.Bind(ep);
        }

        public void Stop() {
            this.socket.Close();
        }

        public void Read() {
            // TODO: what am I gone do here?
        }

        public void Send(UnreliableNetClient client, byte[] message) {
            // TODO: what am I gone do here?
        }

        public void Flush(UnreliableNetClient client) {
            // TODO: what am I gone do here?
        }

        void INetworking<IUDPSocket, UnreliableNetClient>.Read(UnreliableNetClient client) { }
    }
}