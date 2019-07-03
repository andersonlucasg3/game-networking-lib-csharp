using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace Networking.IO {
    public sealed class NetworkingWriter : IWriter {
        private byte[] buffer;
        private bool isSending;

        private readonly Socket socket;

        internal NetworkingWriter(Socket socket) {
            this.socket = socket;
            this.buffer = new byte[0];
        }

        private void Write() {
            if (this.socket.Connected) {
                if (this.isSending) { return; }

                this.isSending = true;

                this.socket.BeginSend(this.buffer, 0, this.buffer.Length, SocketFlags.Partial, (ar) => {
                    int written = this.socket.EndSend(ar);
                    if (written > 0) { this.ShrinkBuffer(written); }
                    this.isSending = false;
                }, this);
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
            public static IWriter Writer(this Socket op) {
                return new NetworkingWriter(op);
            }
        }
    }
}
