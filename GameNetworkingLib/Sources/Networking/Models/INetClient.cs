using System;
using Networking.IO;

namespace Networking.Models {
    public interface INetClientReadListener {
        void ClientDidReadBytes(NetClient client, byte[] bytes);
    }

    public interface INetClient {
        IReader reader { get; }
        IWriter writer { get; }

        bool isConnected { get; }

        INetClientReadListener listener { get; set; }

        void Connect(NetEndPoint endPoint, Action connectAction);
        void Disconnect(Action disconnectAction);
    }
}