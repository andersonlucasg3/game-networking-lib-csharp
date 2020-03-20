using System;
using Networking.Models;

namespace Networking.IO {
    public interface ISocket {
        bool isConnected { get; }
        bool isBound { get; }

        bool isAcceptClientSupported { get; }

        #region Server

        void Bind(NetEndPoint endPoint);
        void Listen(int backlog);
        void Accept(Action<ISocket> callback);
        void Close();

        #endregion

        #region Client

        void Connect(NetEndPoint endPoint, Action callback);
        void Disconnect(Action callback);

        #endregion

        #region Read & Write

        void Read(Action<byte[]> callback);
        void Write(byte[] bytes, Action<int> callback);

        #endregion
    }
}
