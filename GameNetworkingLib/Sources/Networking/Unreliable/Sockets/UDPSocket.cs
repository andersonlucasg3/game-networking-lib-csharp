using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Logging;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace Networking.Sockets {
    public interface IUDPSocket : ISocket, IEquatable<IPEndPoint> {
        void BindToRemote(NetEndPoint endPoint);

        void Close();

        void Read(Action<byte[], IUDPSocket> callback);
    }

    public sealed class UDPSocket : IUDPSocket, IDisposable {
        private const int bufferSize = 1024;

        private Socket socket;
        private IPEndPoint boundEndPoint;
        private readonly IPEndPoint remoteEndPoint;

        private readonly Dictionary<EndPoint, UDPSocket> instantiatedEndPointSockets;

        public bool isBound => this.socket.IsBound;
        public bool isCommunicable { get; private set; }

        public UDPSocket() {
            this.instantiatedEndPointSockets = new Dictionary<EndPoint, UDPSocket>();
        }

        private UDPSocket(Socket socket, IPEndPoint remoteEndPoint) : this() {
            this.socket = socket;

            this.remoteEndPoint = remoteEndPoint;
            this.isCommunicable = true;
        }

        public void Dispose() {
            this.socket.Dispose();
        }

        public void Close() {
            this.socket.Close();
        }

        public void Bind(NetEndPoint endPoint) {
            this.boundEndPoint = this.From(endPoint);
            this.socket = new Socket(this.boundEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            this.socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            this.socket.Bind(this.boundEndPoint);
            this.isCommunicable = true;
        }

        public void BindToRemote(NetEndPoint endPoint) {
            Logger.Log($"Connected to {endPoint}");
            this.socket.Connect(this.From(endPoint));
            this.isCommunicable = true;
        }

        public void Read(Action<byte[], IUDPSocket> callback) {
            var buffer = new byte[bufferSize];
            EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            this.socket.BeginReceiveFrom(buffer, 0, bufferSize, SocketFlags.None, ref endPoint, ar => {
                var readBytes = this.socket.EndReceiveFrom(ar, ref endPoint);

                if (!this.instantiatedEndPointSockets.TryGetValue(endPoint, out UDPSocket socket)) {
                    socket = new UDPSocket(this.socket, endPoint as IPEndPoint);
                    this.instantiatedEndPointSockets[endPoint] = socket;
                }
                byte[] shrinkedBuffer = new byte[readBytes];
                Array.Copy(buffer, shrinkedBuffer, readBytes);

                callback.Invoke(shrinkedBuffer, socket);
            }, null);
        }

        public void Write(byte[] bytes, Action<int> callback) {
            if (bytes.Length == 0) {
                callback.Invoke(0);
                return;
            }

            if (!this.socket.Connected) { return; }

            if (this.remoteEndPoint == null) {
                this.socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, ar => {
                    var writtenCount = this.socket.EndSend(ar);
                    callback.Invoke(writtenCount);
                }, null);
            } else {
                this.socket.BeginSendTo(bytes, 0, bytes.Length, SocketFlags.None, this.remoteEndPoint, ar => {
                    var writtenCount = this.socket.EndSendTo(ar);
                    callback.Invoke(writtenCount);
                }, null);
            }
        }

        public override string ToString() {
            return $"{{EndPoint-{this.remoteEndPoint}}}";
        }

        #region Equatable Methods

        public bool Equals(IPEndPoint endPoint) {
            return this.remoteEndPoint.Equals(endPoint);
        }

        #endregion

        #region Private Methods

        private IPEndPoint From(NetEndPoint ep) {
            return new IPEndPoint(IPAddress.Parse(ep.host), ep.port);
        }

        #endregion
    }
}