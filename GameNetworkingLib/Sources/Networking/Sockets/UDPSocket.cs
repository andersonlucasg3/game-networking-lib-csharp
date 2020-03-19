using System;
using Networking.IO;
using Networking.Models;

namespace Networking.Sockets {
    public class UDPSocket : ISocket {
        public bool isConnected => false;

        public bool isBound => false;

        public bool noDelay { get; set; }
        public bool blocking { get; set; }

        public void Accept(Action<ISocket> acceptAction) {
            throw new NotImplementedException();
        }

        public void Bind(NetEndPoint endPoint) {
            throw new NotImplementedException();
        }

        public void Close() {
            throw new NotImplementedException();
        }

        public void Connect(NetEndPoint endPoint, Action connectAction) {
            throw new NotImplementedException();
        }

        public void Disconnect(Action disconnectAction) {
            throw new NotImplementedException();
        }

        public void Listen(int backlog) {
            throw new NotImplementedException();
        }

        public void Read(Action<byte[]> readAction) {
            throw new NotImplementedException();
        }

        public void Write(byte[] bytes, Action<int> writeAction) {
            throw new NotImplementedException();
        }
    }
}