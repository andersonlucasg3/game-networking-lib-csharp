using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace Networking.IO {
    public sealed class NetworkingReader : IReader {
        private readonly Socket socket;
        private readonly Thread readThread;
        private readonly Queue<byte[]> readQueue;
        private bool reading = true;

        internal NetworkingReader(Socket socket) {
            this.socket = socket;
            this.socket.ReceiveTimeout = 100;

            this.readQueue = new Queue<byte[]>();

            this.readThread = new Thread(() => { this.ReadThreadRun(); });
            this.readThread.Start();
        }

        public byte[] Read() {
            byte[] bytes = new byte[0];
            lock(this) {
                if (this.readQueue.Count > 0) {
                    bytes = this.readQueue.Dequeue();
                }
            }
            return bytes;
        }

        public void Dispose() {
            this.reading = false;
        }

        private void ReadThreadRun() {
            do {
                byte[] shrinked = new byte[0];

                byte[] buffer = new byte[4096];

                int count = 0;
                try {
                    count = this.socket.Receive(buffer);
                } catch { }

                if (count > 0 && count < 4096) {
                    shrinked = new byte[count];
                    this.Copy(buffer, ref shrinked);
                } else if (buffer.Length == count) {
                    shrinked = buffer;
                }

                lock (this) {
                    this.readQueue.Enqueue(shrinked);
                }
            } while (this.reading);
        }

        private void Copy(byte[] source, ref byte[] destination) {
            for (var i = 0; i < destination.Length; i++) {
                destination[i] = source[i];
            }
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