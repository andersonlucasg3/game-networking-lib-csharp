using System;

namespace Networking.IO {
    public interface IReaderDelegate {
        void ClientDidSendBytes(byte[] bytes);
    }

    public interface IReader {
        IReaderDelegate Delegate { get; set; }

        void Read();
    }
}