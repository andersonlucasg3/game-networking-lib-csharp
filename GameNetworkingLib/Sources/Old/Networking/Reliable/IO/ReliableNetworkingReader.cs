using Networking.Commons.IO;
using Networking.Sockets;

namespace Networking.IO {
    public class ReliableNetworkingReader : NetworkingReader<ITCPSocket> {
        private bool isReceiving;

        public ReliableNetworkingReader(ITCPSocket socket) : base(socket) { }

        public override void Receive() {
            if (this.isReceiving) { return; }

            this.isReceiving = true;

            this.socket.Read((buffer, count) => {
                this.listener?.ClientDidRead(buffer, count);

                this.isReceiving = false;
            });
        }
    }
}