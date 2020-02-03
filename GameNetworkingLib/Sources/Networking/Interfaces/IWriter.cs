using System;

namespace Networking.IO {
    public interface IWriter {
        void Write(byte[] data);
        void Flush();
    }
}