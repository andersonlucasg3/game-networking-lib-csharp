using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace Networking.IO {
    public sealed class NetworkingWriter : IWriter {
        private readonly ISocket socket;

        private List<byte> buffer;
        private bool isSending;

        internal NetworkingWriter(ISocket socket) {
            this.socket = socket;
            this.buffer = new List<byte>();
        }

        private void Write() {
            if (this.socket.isConnected) {
                if (this.isSending) { return; }

                this.isSending = true;

                this.socket.Write(this.buffer.ToArray(), (written) => {
                    if (written > 0) { this.ShrinkBuffer(written); }
                    this.isSending = false;
                });
            }
        }

        public void Write(byte[] data) {
            this.buffer.AddRange(data);
            this.Write();
        }

        public void Flush() {
            this.Write();
        }

        private void ShrinkBuffer(int written) {
            this.buffer.RemoveRange(0, written);
        }
    }

    namespace Extensions {
        public static partial class SocketExt {
            public static IWriter Writer(this ISocket op) {
                return new NetworkingWriter(op);
            }
        }
    }
}
