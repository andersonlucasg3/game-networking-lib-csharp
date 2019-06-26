using System.Net.Sockets;

namespace Networking.Models {
    using IO;

    public sealed class Client {
        internal Socket socket;

        internal IReader reader;
        internal IWriter writer;

        public bool IsConnected { get { return this.socket.IsConnected(); } }

        internal Client(Socket socket, IReader reader, IWriter writer) {
            this.socket = socket;
            this.reader = reader;
            this.writer = writer;
        }

        public override bool Equals(object obj) {
            if (obj is Client) {
                return this.socket == ((Client)obj).socket;
            } else if (obj is Socket) {
                return this.socket == obj;
            }
            return object.Equals(this, obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }

    public static class SocketExt {
        public static bool IsConnected(this Socket op) {
            bool part1 = op.Poll(1000, SelectMode.SelectRead);
            bool part2 = op.Available == 0;
            return !(part1 && part2);
        }
    }
}