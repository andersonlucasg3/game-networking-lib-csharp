using System.Net.Sockets;

namespace Networking.IO {
    public sealed class NetworkingReader : IReader {
        private Socket socket;

        internal NetworkingReader(Socket socket) {
            this.socket = socket;
        }

        public byte[] Read() {
            byte[] buffer = new byte[4096];
            int count = this.socket.Receive(buffer);
            if (count < 4096) {
                byte[] shrinked = new byte[count];
                buffer.CopyTo(shrinked, 0);
                return shrinked;
            }
            return buffer;
        }
    }

    namespace Extensions {
        public static partial class SocketExt {
            public static IReader Reader(this Socket op) {
                return new NetworkingReader(op);
            }
        }
    }
}