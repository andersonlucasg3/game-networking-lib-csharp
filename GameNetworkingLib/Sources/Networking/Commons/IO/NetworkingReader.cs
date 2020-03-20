namespace Networking.Commons.IO {
    using Sockets;

    public interface IReader {
        public interface IListener {
            void ClientDidRead(byte[] bytes);
        }

        IListener listener { get; set; }

        void Read();
    }

    public sealed class NetworkingReader : IReader {
        private readonly ISocket socket;

        private bool isReceiving;

        public IReader.IListener listener { get; set; }

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