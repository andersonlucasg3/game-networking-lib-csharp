using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace Networking.IO {
    public sealed class NetworkingWriter : IWriter {
        private readonly ISocket socket;

        private byte[] buffer;
        private bool isSending;

        internal NetworkingWriter(ISocket socket) {
            this.socket = socket;
            this.buffer = new byte[0];
        }

        private void Write() {
            if (this.socket.isConnected) {
                if (this.isSending) { return; }

                this.isSending = true;

                this.socket.Write(this.buffer, (written) => {
                    if (written > 0) { this.ShrinkBuffer(written); }
                    this.isSending = false;
                });
            }
        }

        public void Write(byte[] data) {
            List<byte> bytes = new List<byte>(this.buffer);
            bytes.AddRange(data);
            this.buffer = bytes.ToArray();
            this.Write();
        }

        public void Flush() {
            this.Write();
        }

        private void ShrinkBuffer(int written) {
            List<byte> bytes = new List<byte>(this.buffer);
            bytes.RemoveRange(0, written);
            this.buffer = bytes.ToArray();
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
