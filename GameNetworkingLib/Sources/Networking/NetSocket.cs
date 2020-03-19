using System.Net;
using System.Collections.Generic;

namespace Networking {
    using Models;
    using IO.Extensions;
    using Networking.IO;
    using Logging;

    public sealed class NetSocket : INetworking {
        private readonly ISocket socket;
        private readonly Queue<ISocket> acceptedQueue;
        private bool isAccepting = false;

        public int port { get; private set; }

        public INetworkingListener listener { get; set; }

        public NetSocket(ISocket socket) {
            this.socket = socket;
            this.acceptedQueue = new Queue<ISocket>();
        }

        public void StartServer(int port) {
            this.port = port;
            NetEndPoint ep = new NetEndPoint(IPAddress.Any.ToString(), port);
            this.socket.Bind(ep);
            this.socket.Listen(10);
        }

        private void AcceptNewClient() {
            if (this.isAccepting) { return; }

            this.isAccepting = true;

            this.socket.Accept((accepted) => {
                if (accepted != null) {
                    accepted.noDelay = true;
                    accepted.blocking = false;
                    this.acceptedQueue.Enqueue(accepted);
                }

                this.isAccepting = false;
            });
        }

        public INetClient Accept() {
            this.AcceptNewClient();

            if (this.acceptedQueue.Count > 0) {
                var accepted = this.acceptedQueue.Dequeue();
                return new NetClient(accepted, accepted.Reader(), accepted.Writer());
            }
            return null;
        }

        public void StopServer() {
            this.socket.Close();
        }

        public void Connect(string host, int port) {
            NetClient client = new NetClient(this.socket, this.socket.Reader(), this.socket.Writer());
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

        public void Disconnect(INetClient client) {
            client.Disconnect(() => this.listener?.NetworkingDidDisconnect(client));
        }

        public void Read(INetClient client) {
            client.reader.Read();
        }

        public void Send(INetClient client, byte[] message) {
            client.writer.Write(message);
        }

        public void Flush(INetClient client) {
            if (client.isConnected) {
                client.writer.Flush();
            } else {
                this.listener?.NetworkingDidDisconnect(client);
            }
        }
    }
}
