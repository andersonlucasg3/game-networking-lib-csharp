using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Networking {
    using Commons;
    using Models;
    using IO.Extensions;
    using Networking.IO;

    public sealed class NetSocket : WeakListener<INetworkingListener>, INetworking {
        private readonly ISocket socket;
        private readonly Queue<ISocket> acceptedQueue;

        public int Port { get; private set; }

        public NetSocket(ISocket socket) {
            this.socket = socket;
            this.acceptedQueue = new Queue<ISocket>();
        }

        public void StartServer(int port) {
            this.Port = port;
            NetEndPoint ep = new NetEndPoint(IPAddress.Any.ToString(), port);
            this.socket.Bind(ep);
            this.socket.Listen(10);

            this.AcceptNewClient();
        }

        private void AcceptNewClient() {
            this.socket.Accept((accepted) => {
                accepted.noDelay = true;
                accepted.blocking = false;
                this.acceptedQueue.Enqueue(accepted);

                this.AcceptNewClient();
            });
        }

        public INetClient Accept() {
            if (this.acceptedQueue.Count > 0) {
                var accepted = this.acceptedQueue.Dequeue();
                return new NetClient(accepted, accepted.Reader(), accepted.Writer());
            }
            return null;
        }

        public void Stop() {
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

            Logging.Logger.Log(this.GetType(), "Trying to connect to " + host + "-" + port);
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
