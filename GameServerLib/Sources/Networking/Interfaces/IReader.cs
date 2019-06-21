using System;

namespace Networking.IO {
    public interface IReader: IDisposable {
        byte[] Read();
    }
}