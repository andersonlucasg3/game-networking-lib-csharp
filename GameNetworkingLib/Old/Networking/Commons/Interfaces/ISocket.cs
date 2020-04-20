using System;

namespace Networking.Commons.Sockets {
    public interface ISocket {
        bool isCommunicable { get; }
        bool isBound { get; }

        void Bind(GameNetworking.Sockets.NetEndPoint endPoint);

        void Write(byte[] bytes, Action<int> callback);
    }
}