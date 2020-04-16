using System.Collections.Concurrent;
using Networking.Commons.Sockets;

namespace Networking.Commons.IO {
    public interface IWriter {
        void Write(byte[] data);
        void Flush();
    }

    public abstract class NetworkingWriter<TSocket> : IWriter
        where TSocket : ISocket {
        private readonly TSocket socket;
        private readonly ConcurrentQueue<byte[]> buffer;

        private bool isSending = false;

        internal NetworkingWriter(TSocket socket) {
            this.socket = socket;
            this.buffer = new ConcurrentQueue<byte[]>();
        }

        private void Write() {
            if (this.isSending) { return; }
            this.isSending = true;

            if (!this.buffer.TryDequeue(out byte[] buffer)) { return; }

            this.socket.Write(buffer, (written) => {
                this.isSending = false;
            });
        }

        public void Write(byte[] data) {
            this.buffer.Enqueue(data);
            this.Write();
        }

        public void Flush() {
            this.Write();
        }
    }
}
