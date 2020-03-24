using System;
using System.Collections.Generic;
using Networking.Commons.Models;
using Networking.Sockets;

namespace Test.Core.Model {
    class UnreliableSocketMock : IUDPSocket, IEquatable<UnreliableSocketMock> {
        private static readonly Dictionary<NetEndPoint, List<byte>> sentBytes = new Dictionary<NetEndPoint, List<byte>>();
        private static readonly Dictionary<int, IUDPSocket> socketsMapping = new Dictionary<int, IUDPSocket>();
        private static int sharedSocketIdCounter = 0;

        private readonly int socketId;
        private NetEndPoint boundTo;
        private NetEndPoint boundToRemote;

        public bool isCommunicable { get; private set; }
        public bool isBound { get; private set; }

        public UnreliableSocketMock() {
            this.socketId = sharedSocketIdCounter;
            sharedSocketIdCounter++;
        }

        private UnreliableSocketMock(int id) {
            this.socketId = id;
        }

        public void Bind(NetEndPoint endPoint) {
            this.boundTo = endPoint;
            this.isBound = true;
            this.isCommunicable = true;
        }

        public void BindToRemote(NetEndPoint endPoint) {
            this.isCommunicable = true;
            this.boundToRemote = endPoint;
        }

        public void Close() {
            this.isCommunicable = false;
            this.isBound = false;
        }

        public void Read(Action<byte[], IUDPSocket> callback) {
            void DoTheReading(List<byte> value) {
                var bytes = value.ToArray();
                value.Clear();

                if (socketsMapping.TryGetValue(this.socketId, out IUDPSocket socket)) {
                    callback.Invoke(bytes, socket);
                    return;
                }

                var newSocket = new UnreliableSocketMock(this.socketId);
                socketsMapping[this.socketId] = newSocket;
                callback.Invoke(bytes, newSocket);
            }

            if (sentBytes.TryGetValue(this.boundToRemote, out List<byte> value)) {
                DoTheReading(value);
            } else {
                foreach (var keyValue in sentBytes) {
                    DoTheReading(keyValue.Value);
                }
            }
        }

        public void Write(byte[] bytes, Action<int> callback) {
            if (sentBytes.TryGetValue(this.boundToRemote, out List<byte> value)) {
                value.AddRange(bytes);
                callback?.Invoke(bytes.Length);
                return;
            }
            var list = new List<byte>();
            list.AddRange(bytes);
            sentBytes[this.boundToRemote] = list;
            callback?.Invoke(bytes.Length);
        }

        bool IEquatable<UnreliableSocketMock>.Equals(UnreliableSocketMock other) {
            return this.socketId == other.socketId;
        }
    }
}