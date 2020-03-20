using System;
using System.Collections.Generic;
using Networking.IO;
using Networking.Models;

namespace Tests.Core.Model {
    class SocketMock : ISocket {
        private static readonly Queue<SocketMock> pendingAcceptClients = new Queue<SocketMock>();
        private static readonly List<SocketMock> connectedClients = new List<SocketMock>();

        private byte[] buffer;
        private SocketMock serverCounterPart;

        public bool isConnected { get; private set; }
        public bool isBound { get; private set; }
        public bool isAcceptClientSupported => true;

        public void Bind(NetEndPoint endPoint) {
            this.isBound = true;
        }

        public void Listen(int backlog) {
            pendingAcceptClients.Clear();
            connectedClients.Clear();
        }

        public void Accept(Action<ISocket> acceptAction) {
            if (pendingAcceptClients.TryDequeue(out SocketMock socket)) {
                connectedClients.Add(socket);
            }
            acceptAction?.Invoke(socket);
        }

        public void Close() {
            this.isBound = false;
            this.isConnected = false;
        }

        public void Connect(NetEndPoint endPoint, Action connectAction) {
            this.serverCounterPart = new SocketMock() {
                isConnected = true,
                serverCounterPart = this
            };
            pendingAcceptClients.Enqueue(this.serverCounterPart);
            this.isConnected = true;
            connectAction?.Invoke();
        }

        public void Disconnect(Action disconnectAction) {
            connectedClients.Remove(this);

            var counterPart = this.serverCounterPart;
            this.serverCounterPart = null;
            counterPart?.Disconnect(null);

            this.isConnected = false;
            disconnectAction?.Invoke();
        }

        public void Read(Action<byte[]> readAction) {
            readAction?.Invoke(this.buffer);
            this.buffer = null;
        }

        public void Write(byte[] bytes, Action<int> writeAction) {
            if (this.serverCounterPart.buffer == null) {
                this.serverCounterPart.buffer = bytes;
            } else {
                List<byte> mutableBuffer = new List<byte>(this.serverCounterPart.buffer);
                mutableBuffer.AddRange(bytes);
                this.serverCounterPart.buffer = mutableBuffer.ToArray();
            }
            writeAction?.Invoke(bytes.Length);
        }
    }

}