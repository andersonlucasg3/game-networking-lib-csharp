using Networking.Commons.IO;
using Networking.Sockets;

namespace Networking.IO {
    public class UnreliableNetworkingReader : NetworkingReader<IUDPSocket> {
        public interface IListener {
            void ReaderDidRead(byte[] bytes, int count, IUDPSocket from);
        }

        private bool isReceiving = false;

        public new IListener listener { get; set; }

        public UnreliableNetworkingReader(IUDPSocket socket) : base(socket) { }

        public override void Receive() {
            lock (this) {
                if (this.isReceiving) { return; }
                this.isReceiving = true;
            }

            this.socket.Read((bytes, count, fromSocket) => {
                this.listener?.ReaderDidRead(bytes, count, fromSocket);

                lock (this) { this.isReceiving = false; }
            });
        }
    }
}