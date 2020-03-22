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

        public void Read(UnreliableNetClient client) {
            client.reader.Read();
        }

        public void Send(UnreliableNetClient client, byte[] message) {
            client.writer.Write(message);
        }

        public void Flush(UnreliableNetClient client) {
            client.writer.Flush();
        }
    }
}