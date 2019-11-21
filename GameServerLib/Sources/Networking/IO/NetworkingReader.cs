using System.Net.Sockets;
using System.Collections.Generic;
using Commons;

namespace Networking.IO {
    public sealed class NetworkingReader : WeakDelegate<IReaderDelegate>, IReader {
        private readonly Socket socket;

        private bool isReceiving;
        
        internal NetworkingReader(Socket socket) {
            this.socket = socket;
        }

        private void Receive() {
            if (this.isReceiving) { return; }

            this.isReceiving = true;

            var bufferSize = 4096 * 2;

            byte[] receiveBuffer = new byte[bufferSize];
            this.socket.BeginReceive(receiveBuffer, 0, bufferSize, SocketFlags.Partial, (ar) => {
                int count = this.socket.EndReceive(ar);

                byte[] bytesRead = new byte[count];
                this.Copy(receiveBuffer, ref bytesRead);

                this.Delegate?.ClientDidSendBytes(bytesRead);

                this.isReceiving = false;
            }, this);
        }

        public void Read() {
            this.Receive();
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