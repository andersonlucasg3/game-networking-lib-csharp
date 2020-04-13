using System.Collections.Generic;

namespace Networking.Commons.IO {
    using Sockets;

    public interface IWriter {
        void Write(byte[] data);
        void Flush();
    }

    public abstract class NetworkingWriter<TSocket> : IWriter 
        where TSocket : ISocket {
        private readonly TSocket socket;

        private readonly List<byte> buffer;
        
        internal NetworkingWriter(TSocket socket) {
            this.socket = socket;
            this.buffer = new List<byte>();
        }

        private void Write() {
            if (this.buffer.Count == 0) { return; }

            this.socket.Write(this.buffer.ToArray(), (written) => {
                if (written > 0) { this.ShrinkBuffer(written); }
            });
        }

        public void Write(byte[] data) {
            this.buffer.AddRange(data);
            this.Write();
        }

        public void Flush() {
            this.Write();
        }

        private void ShrinkBuffer(int written) {
            if (this.buffer.Count == 0) { return; }
            this.buffer.RemoveRange(0, written);
        }
    }
}
