using System.Net.Sockets;
using System.Collections.Generic;

namespace Networking.IO {
    public sealed class NetworkingWriter : IWriter {
        private Socket socket;
        private List<byte> buffer;

        internal NetworkingWriter(Socket socket) {
            this.socket = socket;
            this.buffer = new List<byte>();
        }

        public void Write(byte[] data) {
            this.buffer.AddRange(data);
            this.Flush();
        }

        public void Flush() {
            int written = this.socket.Send(this.buffer.ToArray());
            this.ShrinkBuffer(written);
        }

        private void ShrinkBuffer(int written) {
            this.buffer.RemoveRange(0, written);
        }
    }

    namespace Extensions {
        public static partial class SocketExt {
            public static IWriter Writer(this Socket op) {
                return new NetworkingWriter(op);
            }
        }
    }
}