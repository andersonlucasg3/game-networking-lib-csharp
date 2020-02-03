using System;
using Networking.Models;

namespace Networking.IO {
    public interface ISocket {
        bool isConnected { get; }
        bool isBound { get; }

        bool noDelay { get; set; }
        bool blocking { get; set; }

        #region Server

        void Bind(NetEndPoint endPoint);
        void Listen(int backlog);
        void Accept(Action<ISocket> acceptAction);
        void Close();

        #endregion

        #region Client

        void Connect(NetEndPoint endPoint, Action connectAction);
        void Disconnect(Action disconnectAction);

        #endregion

        #region Read & Write

        void Read(Action<byte[]> readAction);
        void Write(byte[] bytes, Action<int> writeAction);

        #endregion
    }
}
