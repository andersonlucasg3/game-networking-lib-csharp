using Networking.Commons.IO;
using Networking.Sockets;

namespace Networking.IO {
    public class UnreliableNetworkingReader : NetworkingReader<IUDPSocket> {
        public interface IListener {
            void ReaderDidRead(byte[] bytes, IUDPSocket from);
        }

        private bool isReceiving = false;

        public new IListener listener { get; set; }

        public UnreliableNetworkingReader(IUDPSocket socket) : base(socket) { }

        public override void Receive() {
            if (this.isReceiving) { return; }
            this.isReceiving = true;

            this.socket.Read((bytes, fromSocket) => {
                this.listener?.ReaderDidRead(bytes, fromSocket);

                this.isReceiving = false;
            });
        }
    }
}