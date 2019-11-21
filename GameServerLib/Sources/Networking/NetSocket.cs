using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Networking {
    using Commons;
    using Models;
    using IO.Extensions;

    public sealed class NetSocket : WeakDelegate<INetworkingDelegate>, INetworking {
        private readonly Socket socket;
        private readonly Queue<Socket> acceptedQueue;

        public int Port { get; private set; }

        public NetSocket() {
            this.acceptedQueue = new Queue<Socket>();

            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) {
                NoDelay = true,
                Blocking = false,
                SendTimeout = 2000,
                ReceiveTimeout = 2000
            };
        }

        public void Start(int port) {
            this.Port = port;
            this.socket.Bind(new IPEndPoint(IPAddress.Any, port));
            this.socket.Listen(10);

            this.AcceptNewClient();
        }

        private void AcceptNewClient() {
            this.socket.BeginAccept((ar) => {
                var accepted = this.socket.EndAccept(ar);
                accepted.NoDelay = true;
                accepted.Blocking = false;
                this.acceptedQueue.Enqueue(accepted);

                this.AcceptNewClient();
            }, this);
        }

        public NetClient Accept() {
            if (this.acceptedQueue.Count > 0) {
                var accepted = this.acceptedQueue.Dequeue();
                return new NetClient(accepted, accepted.Reader(), accepted.Writer());
            }
            return null;
        }

        public void Stop() {
            this.socket.Shutdown(SocketShutdown.Both);
            this.socket.Dispose();
        }

        public void Connect(string host, int port) {
            var result = this.socket.BeginConnect(host, port, (ar) => {
                if (this.socket.Connected) {
                    this.socket.EndConnect(ar);
                    this.Delegate?.NetworkingDidConnect(new NetClient(this.socket, this.socket.Reader(), this.socket.Writer()));
                } else {
                    this.socket.Close();
                    this.Delegate?.NetworkingConnectDidTimeout();
                }
            }, null);

            Logging.Logger.Log(this.GetType(), "Trying to connect to " + host + "-" + port);
        }

        public void Disconnect(NetClient client) {
            client.socket.BeginDisconnect(false, (ar) => {
                client.socket.EndDisconnect(ar);
                this.Delegate?.NetworkingDidDisconnect(client);
            }, this);
        }

        public void Read(NetClient client) {
            client.reader.Read();
        }

        public void Send(NetClient client, byte[] message) {
            client.writer.Write(message);
        }

        public void Flush(NetClient client) {
            if (client.IsConnected) {
                client.writer.Flush();
            } else {
                this.Delegate?.NetworkingDidDisconnect(client);
            }
        }
    }
}
