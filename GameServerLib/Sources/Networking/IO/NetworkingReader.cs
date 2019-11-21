using System.Net.Sockets;
using System.Collections.Generic;
using Commons;

namespace Networking.IO {
    public sealed class NetworkingReader : WeakDelegate<IReaderDelegate>, IReader {
        private readonly Socket socket;
        private bool hasStartedReading = false;
        
        internal NetworkingReader(Socket socket) {
            this.socket = socket;
        }

        private void Receive() {
            var bufferSize = 4096 * 2;

            byte[] receiveBuffer = new byte[bufferSize];
            this.socket.BeginReceive(receiveBuffer, 0, bufferSize, SocketFlags.Partial, (ar) => {
                int count = this.socket.EndReceive(ar);

                byte[] bytesRead = new byte[count];
                this.Copy(receiveBuffer, ref bytesRead);

                this.Delegate?.ClientDidSendBytes(bytesRead);

                if (socket.Connected) {
                    Receive();
                }
            }, this);
        }

        public void Read() {
            if (!hasStartedReading) {
                hasStartedReading = true;
                this.Receive();
            }
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