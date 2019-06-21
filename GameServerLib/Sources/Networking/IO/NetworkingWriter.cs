using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace Networking.IO {
    public sealed class NetworkingWriter : IWriter {
        private readonly Socket socket;
        private readonly List<byte> buffer;
        private readonly Thread writeThread;
        private bool writing = true;

        internal NetworkingWriter(Socket socket) {
            this.socket = socket;
            this.buffer = new List<byte>();

            this.writeThread = new Thread(() => { this.WriteThreadRun(); });
            this.writeThread.Start();
        }

        public void Write(byte[] data) {
            lock (this) { this.buffer.AddRange(data); }
        }

        public void Dispose() {
            this.writing = false;
        }

        private void ShrinkBuffer(int written) {
            this.buffer.RemoveRange(0, written);
        }

        private void WriteThreadRun() {
            do {
                lock(this) {
                    int written = 0;
                    try {
                        written = this.socket.Send(this.buffer.ToArray());
                    } catch { }

                    if (written > 0) { this.ShrinkBuffer(written); }
                }
            } while (this.writing);
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
