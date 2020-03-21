using Networking.Commons;
using Networking.Models;

namespace Networking.Sockets {
    public class UnreliableSocket : INetworking<IUDPSocket, UnreliableNetClient> {
        public int port => throw new System.NotImplementedException();

        public void Flush(UnreliableNetClient client) {
            throw new System.NotImplementedException();
        }

        public void Read(UnreliableNetClient client) {
            throw new System.NotImplementedException();
        }

        public void Send(UnreliableNetClient client, byte[] message) {
            throw new System.NotImplementedException();
        }

        public void Start(int port) {
            throw new System.NotImplementedException();
        }

        public void Stop() {
            throw new System.NotImplementedException();
        }
    }
}