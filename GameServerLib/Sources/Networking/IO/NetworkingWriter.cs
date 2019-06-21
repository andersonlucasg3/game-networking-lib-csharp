using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace Networking.IO {
    public sealed class NetworkingWriter : IWriter {
        private readonly Socket socket;
        private readonly List<byte> buffer;

        private bool isSending;

        internal NetworkingWriter(Socket socket) {
            this.socket = socket;
            this.buffer = new List<byte>();
        }

        private void Write() {
            if (!this.isSending) { this.isSending = true; } else { return; }
            this.socket.BeginSend(this.buffer.ToArray(), 0, this.buffer.Count, SocketFlags.Partial, (ar) => {
                int written = this.socket.EndSend(ar);
                if (written > 0) { this.ShrinkBuffer(written); }
                this.isSending = false;
            }, this);
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
            public static IWriter Writer(this Socket op) {
                return new NetworkingWriter(op);
            }
        }
    }
}
