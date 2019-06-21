using System.Net;
using System.Net.Sockets;

namespace Networking {
    using Models;
    using IO;
    using IO.Extensions;

    public sealed class Networking : INetworking {
        private Socket socket;

        public int Port { get; private set; }

        public Networking() {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.socket.NoDelay = true;
        }

        public void Start(int port) {
            this.Port = port;
            this.socket.Bind(new IPEndPoint(IPAddress.Any, port));
            this.socket.Listen(10);
        }

        public Client Connect(string host, int port) {
            this.socket.Connect(host, port);
            this.socket.Blocking = false;
            return new Client(this.socket, this.socket.Reader(), this.socket.Writer());
        }

        public Client Accept() {
            Socket clientSocket = this.socket.Accept();
            clientSocket.Blocking = false;
            return new Client(clientSocket, clientSocket.Reader(), clientSocket.Writer());
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
