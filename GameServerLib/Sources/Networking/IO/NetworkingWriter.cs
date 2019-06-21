using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace Networking.IO {
    public sealed class NetworkingWriter : IWriter {
        private readonly Socket socket;
        private readonly List<byte> buffer;
        private readonly Thread writeThread;

        internal NetworkingWriter(Socket socket) {
            this.socket = socket;
            this.buffer = new List<byte>();

            this.writeThread = new Thread(() => { this.WriteThreadRun(); });
            this.writeThread.Start();
        }

        ~NetworkingWriter() {
            this.writeThread.Interrupt();
            this.writeThread.Abort();
        }

        public void Write(byte[] data) {
            lock (this) { this.buffer.AddRange(data); }
        }

        private void ShrinkBuffer(int written) {
            this.buffer.RemoveRange(0, written);
        }

        private void WriteThreadRun() {
            while (this.writeThread.IsAlive) {
                lock(this) {
                    int written = this.socket.Send(this.buffer.ToArray());
                    this.ShrinkBuffer(written);
                }
            }
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
