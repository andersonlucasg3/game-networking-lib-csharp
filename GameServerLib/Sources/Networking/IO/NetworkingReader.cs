using System.Net.Sockets;
using System.Collections.Generic;

namespace Networking.IO {
    public sealed class NetworkingReader : IReader {
        private readonly Socket socket;
        private byte[] buffer;

        private bool isReceiving;
        
        internal NetworkingReader(Socket socket) {
            this.socket = socket;
            
            this.buffer = new byte[0];
        }

        private void Receive() {
            if (this.isReceiving) { return; }

            this.isReceiving = true;

            var bufferSize = 4096 * 2;

            byte[] receiveBuffer = new byte[bufferSize];
            this.socket.BeginReceive(receiveBuffer, 0, bufferSize, SocketFlags.Partial, (ar) => {
                int count = this.socket.EndReceive(ar);

                List<byte> listBuffer = new List<byte>(this.buffer);
                if (count > 0 && count < bufferSize) {
                    byte[] shrinked = new byte[count];
                    this.Copy(receiveBuffer, ref shrinked);
                    listBuffer.AddRange(shrinked);
                } else if (receiveBuffer.Length == count) {
                    listBuffer.AddRange(receiveBuffer);
                }
                this.buffer = listBuffer.ToArray();

                this.isReceiving = false;
            }, this);
        }

        public byte[] Read() {
            this.Receive();

            if (this.buffer.Length > 0) {
                var retBuffer = this.buffer;
                this.buffer = new byte[0];
                return retBuffer;
            }
            return new byte[0];
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