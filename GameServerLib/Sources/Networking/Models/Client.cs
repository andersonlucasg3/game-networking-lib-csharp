using System.Net.Sockets;

namespace Networking.Models {
    using IO;
    using Commons;

    public interface INetClientReadDelegate {
        void ClientDidReadBytes(NetClient client, byte[] bytes);
    }

    public sealed class NetClient: WeakDelegate<INetClientReadDelegate>, IReaderDelegate {
        internal Socket socket;

        internal IReader reader;
        internal IWriter writer;

        public bool IsConnected { get { return this.socket.Connected; } }

        internal NetClient(Socket socket, IReader reader, IWriter writer) {
            this.socket = socket;
            this.reader = reader;
            this.writer = writer;

            this.reader.Delegate = this;
        }

        public override bool Equals(object obj) {
            if (obj is NetClient) {
                return this.socket == ((NetClient)obj).socket;
            }
            if (obj is Socket) {
                return this.socket == obj;
            }
            return object.Equals(this, obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        void IReaderDelegate.ClientDidSendBytes(byte[] bytes) {
            Delegate?.ClientDidReadBytes(this, bytes);
        }
    }
}