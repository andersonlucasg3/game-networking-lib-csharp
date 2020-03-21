using System;

namespace Networking.Commons.Sockets {
    using Models;

    public interface ISocket {
        bool isCommunicable { get; }
        bool isBound { get; }

        void Bind(NetEndPoint endPoint);

        void Read(Action<byte[]> callback);
        void Write(byte[] bytes, Action<int> callback);
    }
}