using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Networking {
    using IO;
    using Models;
    using IO.Extensions;

    public sealed class Networking : INetworking {
        private readonly Socket socket;
        private readonly Queue<Socket> acceptedQueue;

        private WeakReference weakDelegate;
        
        public int Port { get; private set; }

        public INetworkingDelegate Delegate {
            get { return this.weakDelegate?.Target as INetworkingDelegate; }
            set { this.weakDelegate = new WeakReference(value); }
        }

        public Networking() {
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

        public Client Accept() {
            if (this.acceptedQueue.Count > 0) {
                var accepted = this.acceptedQueue.Dequeue();
                return new Client(accepted, accepted.Reader(), accepted.Writer());
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
                    this.Delegate?.NetworkingDidConnect(new Client(this.socket, this.socket.Reader(), this.socket.Writer()));
                } else {
                    this.socket.Close();
                    this.Delegate?.NetworkingConnectDidTimeout();
                }
            }, null);

            Logging.Logger.Log(this.GetType(), "Trying to connect to " + host + "-" + port);
        }

        public void Disconnect(Client client) {
            client.socket.BeginDisconnect(false, (ar) => {
                client.socket.EndDisconnect(ar);
                this.Delegate?.NetworkingDidDisconnect(client);
            }, this);
        }

        public byte[] Read(Client client) {
            return client.reader.Read();
        }

        public void Send(Client client, byte[] message) {
            client.writer.Write(message);
        }

        public void Flush(Client client) {
            if (client.IsConnected) {
                client.writer.Flush();
            } else {
                this.Delegate?.NetworkingDidDisconnect(client);
            }
        }
    }
}
