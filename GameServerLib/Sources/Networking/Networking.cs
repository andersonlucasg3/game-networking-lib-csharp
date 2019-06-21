using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace Networking {
    using Models;
    using IO;
    using IO.Extensions;

    public sealed class Networking : INetworking {
        private bool isListening;

        private readonly Socket socket;
        private readonly Thread acceptThread;

        private readonly Queue<Socket> acceptedQueue;

        public int Port { get; private set; }

        public Networking() {
            this.acceptedQueue = new Queue<Socket>();

            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) {
                NoDelay = true
            };

            this.acceptThread = new Thread(() => { this.AcceptThreadRun(); });
            this.acceptThread.Start();
        }

        ~Networking() {
            this.acceptThread.Interrupt();
            this.acceptThread.Abort();
        }

        public void Start(int port) {
            this.Port = port;
            this.socket.Bind(new IPEndPoint(IPAddress.Any, port));
            this.socket.Listen(10);

            this.isListening = true;
        }

        public Client Connect(string host, int port) {
            this.socket.Connect(host, port);
            return new Client(this.socket, this.socket.Reader(), this.socket.Writer());
        }

        public Client Accept() {
            Socket accepted;
            lock(this) { accepted = this.acceptedQueue.Dequeue(); }
            return new Client(accepted, accepted.Reader(), accepted.Writer());
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

        private void AcceptThreadRun() {
            while (this.acceptThread.IsAlive) {
                if (!isListening) { continue; }

                Socket accepted = this.socket.Accept();
                lock(this) { this.acceptedQueue.Enqueue(accepted); }
            }
        }
    }

    namespace Models {
        public partial class Client {
            internal Socket Socket { get { return this.raw as Socket; } }

            internal Client(Socket socket, IReader reader, IWriter writer): this(socket as object, reader, writer) { }
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
