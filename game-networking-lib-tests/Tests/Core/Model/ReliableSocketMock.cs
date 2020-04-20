#if !UNITY_64

using System;
using System.Collections.Generic;
using Networking.Sockets;

namespace Tests.Core.Model {
    class ReliableSocketMock : ITCPSocket {
        private static readonly Queue<ReliableSocketMock> pendingAcceptClients = new Queue<ReliableSocketMock>();
        private static readonly List<ReliableSocketMock> connectedClients = new List<ReliableSocketMock>();

        private byte[] buffer;
        private ReliableSocketMock serverCounterPart;

        public bool isConnected { get; private set; }
        public bool isBound { get; private set; }

        public bool isCommunicable => this.isConnected;

        public void Bind(GameNetworking.Sockets.NetEndPoint endPoint) {
            this.isBound = true;
        }

        public void Listen(int backlog) {
            pendingAcceptClients.Clear();
            connectedClients.Clear();
        }

        public void Accept(Action<ITCPSocket> acceptAction) {
            if (pendingAcceptClients.TryDequeue(out ReliableSocketMock socket)) {
                connectedClients.Add(socket);
            }
            acceptAction?.Invoke(socket);
        }

        public void Close() {
            this.isBound = false;
            this.isConnected = false;
        }

        public void Connect(GameNetworking.Sockets.NetEndPoint endPoint, Action connectAction) {
            this.serverCounterPart = new ReliableSocketMock() {
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

        public void Read(Action<byte[], int> readAction) {
            readAction?.Invoke(this.buffer, this.buffer.Length);
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

#endif