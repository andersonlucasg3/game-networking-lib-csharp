using System.Net;
using Networking.Commons;
using Networking.Commons.Models;
using Networking.IO;
using Networking.Models;

namespace Networking.Sockets {
    public class UnreliableSocket : INetworking<IUDPSocket, UnreliableNetClient>, UnreliableNetworkingReader.IListener {
        public interface IListener {
            void SocketDidRead(byte[] bytes, UnreliableNetClient client);
        }

        private readonly IUDPSocket socket;
        private readonly UnreliableNetworkingReader reader;
        
        public int port { get; private set; }

        public IListener listener { get; set; }

        public UnreliableSocket(IUDPSocket socket) {
            this.socket = socket;
            this.reader = new UnreliableNetworkingReader(socket);
        }

        public void Start(int port) {
            this.port = port;
            NetEndPoint ep = new NetEndPoint(IPAddress.Any.ToString(), port);
            this.socket.Bind(ep);
        }

        public void Stop() {
            this.socket.Close();
        }

        public void BindToRemote(NetEndPoint endPoint) {
            this.socket.BindToRemote(endPoint);
        }

        public void Read() {
            this.reader.Read();
        }

        public void Send(UnreliableNetClient client, byte[] message) {
            client.Write(message);
        }

        public void Flush(UnreliableNetClient client) {
            client.Flush();
        }

        void INetworking<IUDPSocket, UnreliableNetClient>.Read(UnreliableNetClient client) { }
        
        void UnreliableNetworkingReader.IListener.ReaderDidRead(byte[] bytes, IUDPSocket from) {
            this.listener?.SocketDidRead(bytes, new UnreliableNetClient(from));
        }
    }
}