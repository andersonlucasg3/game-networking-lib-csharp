using System.Net.Sockets;
using System.Collections.Generic;
using Commons;

namespace Networking.IO {
    public sealed class NetworkingReader : WeakListener<IReaderListener>, IReader {
        private readonly ISocket socket;

        private bool isReceiving;

        internal NetworkingReader(ISocket socket) {
            this.socket = socket;
        }

        private void Receive() {
            if (this.isReceiving) { return; }

            this.isReceiving = true;

            this.socket.Read((buffer) => {
                this.listener?.ClientDidRead(buffer);

                this.isReceiving = false;
            });
        }

        public void Read() {
            this.Receive();
        }
    }

    namespace Extensions {
        public static partial class SocketExt {
            public static IReader Reader(this ISocket op) {
                return new NetworkingReader(op);
            }
        }
    }
}