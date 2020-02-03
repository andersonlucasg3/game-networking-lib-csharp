using System;

namespace Networking.IO {
    public interface IReaderListener {
        void ClientDidRead(byte[] bytes);
    }

    public interface IReader {
        IReaderListener listener { get; set; }

        void Read();
    }
}