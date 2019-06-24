using System.Net.Sockets;
using System.Collections.Generic;

namespace Networking.IO {
    public sealed class NetworkingReader : IReader {
        private readonly Socket socket;
        private readonly Queue<byte[]> readQueue;

        private bool isReceiving;
        
        internal NetworkingReader(Socket socket) {
            this.socket = socket;
            
            this.readQueue = new Queue<byte[]>();
        }

        private void Receive() {
            if (this.isReceiving) { return; } else { this.isReceiving = true; }

            byte[] buffer = new byte[4096];
            this.socket.BeginReceive(buffer, 0, 4096, SocketFlags.Partial, (ar) => {
                int count = this.socket.EndReceive(ar);

                if (count > 0 && count < 4096) {
                    byte[] shrinked = new byte[count];
                    this.Copy(buffer, ref shrinked);
                    this.readQueue.Enqueue(shrinked);
                } else if (buffer.Length == count) {
                    this.readQueue.Enqueue(buffer);
                }

                this.isReceiving = false;
            }, this);
        }

        public byte[] Read() {
            this.Receive();

            byte[] bytes = new byte[0];
            if (this.readQueue.Count > 0) {
                bytes = this.readQueue.Dequeue();
            }
            return bytes;
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