namespace Networking.Commons.IO {
    using Sockets;

    public interface IReader {
        public interface IListener {
            void ClientDidRead(byte[] bytes);
        }

        IListener listener { get; set; }

        void Receive();
    }

    public abstract class NetworkingReader<TSocket> : IReader
        where TSocket : ISocket {
        protected TSocket socket { get; }

        public IReader.IListener listener { get; set; }

        internal NetworkingReader(TSocket socket) {
            this.socket = socket;
        }

        public abstract void Receive();
    }
}