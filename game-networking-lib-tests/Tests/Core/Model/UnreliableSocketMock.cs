using System;
using System.Collections.Generic;
using System.Net;
using GameNetworking.Sockets;
using Networking.Sockets;

namespace Test.Core.Model {
    class IdentifiableBytes {
        public readonly int fromSocketId;
        public readonly NetEndPoint fromEndPoint;
        public readonly NetEndPoint toEndPoint;
        public readonly List<byte> bytes;

        public IdentifiableBytes(int id, NetEndPoint from, NetEndPoint to, List<byte> bytes) {
            this.fromSocketId = id;
            this.fromEndPoint = from;
            this.toEndPoint = to;
            this.bytes = bytes;
        }
    }

    class UnreliableSocketMock : IUDPSocket, IEquatable<UnreliableSocketMock> {
        private static int sharedSocketCounter = 0;
        private readonly static List<IdentifiableBytes> writtenBytes = new List<IdentifiableBytes>();

        private readonly int socketId;

        private readonly Dictionary<int, UnreliableSocketMock> socketMapping = new Dictionary<int, UnreliableSocketMock>();

        internal NetEndPoint selfEndPoint { get; private set; }
        internal NetEndPoint talkingToEndPoint { get; private set; }

        public bool isCommunicable { get; private set; }
        public bool isBound { get; private set; }

        public UnreliableSocketMock() {
            this.socketId = sharedSocketCounter;
            sharedSocketCounter++;
        }

        public UnreliableSocketMock(int id) {
            this.socketId = id;
        }

        public static void Setup() {
            writtenBytes.Clear();
        }

        public void Bind(NetEndPoint endPoint) {
            this.selfEndPoint = endPoint;
            this.isBound = true;
            this.isCommunicable = true;
        }

        public void BindToRemote(NetEndPoint endPoint) {
            this.talkingToEndPoint = endPoint;
            this.isCommunicable = true;
        }

        public void Unbind() {
            // TODO: does not yet...
        }

        public void Close() {
            this.isBound = false;
            this.isCommunicable = false;
        }

        public void Read(Action<byte[], int, IUDPSocket> callback) {
            var identifiable = writtenBytes.Find(id => id.toEndPoint == this.selfEndPoint && id.bytes.Count > 0);
            if (identifiable == null) {
                callback?.Invoke(null, 0, null);
                return;
            }
            var bytes = identifiable.bytes.ToArray();
            if (this.socketMapping.TryGetValue(identifiable.fromSocketId, out UnreliableSocketMock value)) {
                callback?.Invoke(bytes, bytes.Length, value);
            } else {
                var newSocket = new UnreliableSocketMock(identifiable.fromSocketId) { selfEndPoint = this.selfEndPoint };
                newSocket.BindToRemote(identifiable.fromEndPoint);
                this.socketMapping.Add(identifiable.fromSocketId, newSocket);
                callback?.Invoke(bytes, bytes.Length, newSocket);
            }
            identifiable.bytes.Clear();
        }

        public void Write(byte[] bytes, Action<int> callback) {
            var identifiable = writtenBytes.Find(id => id.toEndPoint == this.talkingToEndPoint && id.fromEndPoint == this.selfEndPoint);
            if (identifiable == null) {
                identifiable = new IdentifiableBytes(this.socketId, this.selfEndPoint, this.talkingToEndPoint, new List<byte>());
                writtenBytes.Add(identifiable);
            }
            identifiable.bytes.AddRange(bytes);
            callback?.Invoke(bytes.Length);
        }

        bool IEquatable<UnreliableSocketMock>.Equals(UnreliableSocketMock other) {
            return this.socketId == other.socketId;
        }

        public bool Equals(IPEndPoint other) {
            return this.talkingToEndPoint.Equals(other);
        }

        public override string ToString() {
            return $"{{EndPoint-{this.talkingToEndPoint}}}";
        }
    }
}