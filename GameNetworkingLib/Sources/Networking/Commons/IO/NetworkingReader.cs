namespace Networking.Commons.IO {
    using Sockets;

    public interface IReaderListener {
        void ClientDidRead(byte[] bytes, int count);
    }
    
    public interface IReader {
        IReaderListener listener { get; set; }

        void Receive();
    }

    public abstract class NetworkingReader<TSocket> : IReader
        where TSocket : ISocket {
        protected TSocket socket { get; }

        public IReaderListener listener { get; set; }

        internal NetworkingReader(TSocket socket) {
            this.socket = socket;
        }

        public abstract void Receive();
    }
}