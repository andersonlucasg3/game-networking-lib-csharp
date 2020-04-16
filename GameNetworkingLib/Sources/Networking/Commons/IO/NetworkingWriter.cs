using System.Collections.Generic;
using Networking.Commons.Sockets;

namespace Networking.Commons.IO {
    public interface IWriter {
        void Write(byte[] data);
        void Flush();
    }

    public abstract class NetworkingWriter<TSocket> : IWriter
        where TSocket : ISocket {
        private readonly TSocket socket;
        private readonly Queue<byte[]> buffer;

        private bool isSending = false;

        internal NetworkingWriter(TSocket socket) {
            this.socket = socket;
            this.buffer = new Queue<byte[]>();
        }

        private void WritePriv(byte[] buffer) {
            if (this.isSending) { return; }
            this.isSending = true;

            if (buffer == null && !this.buffer.TryPeek(out buffer)) { return; }

            this.socket.Write(buffer, (written) => {
                if (buffer.Length != written && written != 0) { throw new System.Exception("Deu merda!"); }
                if (buffer.Length == written && this.buffer.Count > 0) { this.buffer.Dequeue(); }
                this.isSending = false;
            });
        }

        public void Write(byte[] data) {
            if (this.isSending) {
                this.buffer.Enqueue(data);
            } else {
                this.WritePriv(data);
            }
        }

        public void Flush() {
            if (this.buffer.Count == 0) { return; }
            this.WritePriv(null);
        }
    }
}
