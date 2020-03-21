using System.Net;
using System.Collections.Generic;

namespace Networking {
    using Models;
    using Sockets;
    using Logging;
    using Commons;
    using Commons.Models;
    using Networking.IO;

    public interface IReliableSocket : INetworking<ITCPSocket, ReliableNetClient> {

        public interface IListener {
            void NetworkingDidConnect(ReliableNetClient client);
            void NetworkingConnectDidTimeout();
            void NetworkingDidDisconnect(ReliableNetClient client);
        }

        IListener listener { get; set; }

        ReliableNetClient Accept();

        void Connect(string host, int port);
        void Disconnect(ReliableNetClient client);
    }

    public sealed class ReliableSocket : IReliableSocket {
        private readonly ITCPSocket socket;
        private readonly Queue<ITCPSocket> acceptedQueue;
        private bool isAccepting = false;

        public int port { get; private set; }

        public IReliableSocket.IListener listener { get; set; }

        public ReliableSocket(ITCPSocket socket) {
            this.socket = socket;
            this.acceptedQueue = new Queue<ITCPSocket>();
        }

        public void Start(int port) {
            this.port = port;
            NetEndPoint ep = new NetEndPoint(IPAddress.Any.ToString(), port);
            this.socket.Bind(ep);
            this.socket.Listen(10);
        }

        private void AcceptNewClient() {
            if (this.isAccepting) { return; }

            this.isAccepting = true;

            this.socket.Accept((accepted) => {
                if (accepted != null) { this.acceptedQueue.Enqueue(accepted); }
                this.isAccepting = false;
            });
        }

        public ReliableNetClient Accept() {
            this.AcceptNewClient();

            if (this.acceptedQueue.Count > 0) {
                return this.CreateNetClient(this.acceptedQueue.Dequeue());
            }
            return null;
        }

        public void Stop() {
            this.socket.Close();
        }

        public void Connect(string host, int port) {
            ReliableNetClient client = this.CreateNetClient(this.socket);
            NetEndPoint ep = new NetEndPoint(host, port);
            client.Connect(ep, () => {
                if (client.isConnected) {
                    this.listener?.NetworkingDidConnect(client);
                } else {
                    this.socket.Close();
                    this.listener?.NetworkingConnectDidTimeout();
                }
            });

            Logger.Log($"Trying to connect to {host}-{port}");
        }

        public void Disconnect(ReliableNetClient client) {
            client.Disconnect(() => this.listener?.NetworkingDidDisconnect(client));
        }

        public void Read(ReliableNetClient client) {
            client.reader.Read();
        }

        public void Send(ReliableNetClient client, byte[] message) {
            client.writer.Write(message);
        }

        public void Flush(ReliableNetClient client) {
            if (client.isConnected) {
                client.writer.Flush();
            } else {
                this.listener?.NetworkingDidDisconnect(client);
            }
        }

        private ReliableNetClient CreateNetClient(ITCPSocket socket) {
            return new ReliableNetClient(socket, new ReliableNetworkingReader(socket), new ReliableNetworkingWriter(socket));
        }
    }
}
