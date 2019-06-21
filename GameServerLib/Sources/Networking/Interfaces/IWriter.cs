using System;

namespace Networking.IO {
    public interface IWriter: IDisposable {
        void Write(byte[] data);
    }
}