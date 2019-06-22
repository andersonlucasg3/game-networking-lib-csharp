using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Networking {
    using Models;
    using IO;
    using IO.Extensions;

    public sealed class Networking : INetworking {
        private readonly Socket socket;
        private readonly Queue<Socket> acceptedQueue;
        
        public int Port { get; private set; }

        public Networking() {
            this.acceptedQueue = new Queue<Socket>();

            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) {
                NoDelay = true,
                Blocking = false
            };
        }

        public void Start(int port) {
            this.Port = port;
            this.socket.Bind(new IPEndPoint(IPAddress.Any, port));
            this.socket.Listen(10);

            this.socket.BeginAccept((ar) => {
                var accepted = this.socket.EndAccept(ar);
                accepted.Blocking = false;
                accepted.NoDelay = true;
                this.acceptedQueue.Enqueue(accepted);
            }, this);
        }

        public void Stop() {
            this.socket.Shutdown(SocketShutdown.Both);
            this.socket.Dispose();
        }

        public void Connect(string host, int port, NetworkingConnectDelegate connectDelegate) {
            var result = this.socket.BeginConnect(host, port, (ar) => {
                this.socket.EndConnect(ar);
                connectDelegate.Invoke(new Client(this.socket, this.socket.Reader(), this.socket.Writer()));
            }, this);

            Logging.Logger.Log(this.GetType(), "Trying to connect to " + host + "-" + port);
        }



        public Client Accept() {
            if (this.acceptedQueue.Count > 0) {
                var accepted = this.acceptedQueue.Dequeue();
                return new Client(accepted, accepted.Reader(), accepted.Writer());
            }
            return null;
        }

        public void Disconnect(Client client) {
            client.Socket.Shutdown(SocketShutdown.Both);
            client.Socket.Dispose();
        }

        public byte[] Read(Client client) {
            return client.reader.Read();
        }

        public void Send(Client client, byte[] message) {
            client.writer.Write(message);
        }

        public void Flush(Client client) {
            client.writer.Flush();
        }
    }

    namespace Models {
        public partial class Client {
            internal Socket Socket { get { return this.raw as Socket; } }

            internal Client(Socket socket, IReader reader, IWriter writer): this(socket as object, reader, writer) {
                
            }
        }

        public static class SocketExt {
            public static bool IsConnected(this Socket op) {
                bool part1 = op.Poll(1000, SelectMode.SelectRead);
                bool part2 = (op.Available == 0);
                if (part1 && part2) {
                    return false;
                }
                return true;
            }
        }
    }
}
