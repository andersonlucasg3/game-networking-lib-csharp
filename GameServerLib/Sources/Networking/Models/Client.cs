using System.Net.Sockets;

namespace Networking.Models {
    using IO;

    public sealed class NetClient {
        internal Socket socket;

        internal IReader reader;
        internal IWriter writer;

        public bool IsConnected { get { return this.socket.Connected; } }

        internal NetClient(Socket socket, IReader reader, IWriter writer) {
            this.socket = socket;
            this.reader = reader;
            this.writer = writer;
        }

        public override bool Equals(object obj) {
            if (obj is NetClient) {
                return this.socket == ((NetClient)obj).socket;
            } else if (obj is Socket) {
                return this.socket == obj;
            }
            return object.Equals(this, obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}