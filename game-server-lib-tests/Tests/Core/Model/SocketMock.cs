using System;
using System.Collections.Generic;
using Networking.IO;
using Networking.Models;

namespace Tests.Core.Model {
    class SocketMock : ISocket {
        private static Queue<SocketMock> pendingAcceptClients = new Queue<SocketMock>();
        private static List<SocketMock> connectedClients = new List<SocketMock>();
        private static Dictionary<SocketMock, byte[]> socketBuffers = new Dictionary<SocketMock, byte[]>();

        public bool isConnected { get; private set; }
        public bool isBound { get; private set; }

        public bool noDelay { get; set; }
        public bool blocking { get; set; }

        public void Bind(NetEndPoint endPoint) {
            this.isBound = true;
        }

        public void Listen(int backlog) {
            pendingAcceptClients.Clear();
            connectedClients.Clear();
            socketBuffers.Clear();
        }

        public void Accept(Action<ISocket> acceptAction) {
            if (pendingAcceptClients.TryDequeue(out SocketMock socket)) {
                connectedClients.Add(socket);
                acceptAction?.Invoke(socket);
            }
        }

        public void Close() {
            this.isBound = false;
            this.isConnected = false;
        }

        public void Connect(NetEndPoint endPoint, Action connectAction) {
            pendingAcceptClients.Enqueue(this);
            this.isConnected = true;
            connectAction?.Invoke();
        }

        public void Disconnect(Action disconnectAction) {
            connectedClients.Remove(this);
            this.isConnected = false;
            disconnectAction?.Invoke();
        }

        public void Read(Action<byte[]> readAction) {
            socketBuffers.TryGetValue(this, out byte[] buffer);
            readAction?.Invoke(buffer);
        }

        public void Write(byte[] bytes, Action<int> writeAction) {
            if (socketBuffers.TryGetValue(this, out byte[] buffer)) {
                List<byte> mutableBuffer = new List<byte>(buffer);
                mutableBuffer.AddRange(bytes);
                socketBuffers[this] = mutableBuffer.ToArray();
            } else {
                socketBuffers[this] = bytes;
            }
            writeAction?.Invoke(bytes.Length);
        }
    }

}