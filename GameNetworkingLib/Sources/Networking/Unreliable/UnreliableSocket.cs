using System.Collections.Generic;
using Networking.Commons;
using Networking.Commons.Models;
using Networking.IO;
using Networking.Models;

namespace Networking.Sockets {
    public class UnreliableSocket : INetworking<IUDPSocket, UnreliableNetClient>, UnreliableNetworkingReader.IListener {
        public interface IListener {
            void SocketDidRead(byte[] bytes, UnreliableNetClient client);
        }

        private readonly UnreliableNetworkingReader reader;
        private readonly Dictionary<IUDPSocket, UnreliableNetClient> socketClientCollection = new Dictionary<IUDPSocket, UnreliableNetClient>();

        internal IUDPSocket socket { get; private set; }
        
        public int port { get; private set; }

        public IListener listener { get; set; }

        public UnreliableSocket(IUDPSocket socket) {
            this.socket = socket;
            this.reader = new UnreliableNetworkingReader(socket) { listener = this };
        }

        public void Start(string host, int port) {
            this.port = port;
            this.socket.Bind(new NetEndPoint(host, port));
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
            if (from == null) { return; }

            if (!this.socketClientCollection.TryGetValue(from, out UnreliableNetClient client)) {
                client = new UnreliableNetClient(from);
                this.socketClientCollection[from] = client;
            }
            this.listener?.SocketDidRead(bytes, client);
        }
    }
}