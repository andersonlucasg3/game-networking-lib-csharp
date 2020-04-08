using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace Networking.Sockets {
    public interface IUDPSocket : ISocket {
        void BindToRemote(NetEndPoint endPoint);

        void Close();

        void Read(Action<byte[], IUDPSocket> callback);
    }

    public sealed class UDPSocket : IUDPSocket, IDisposable {
        private const int bufferSize = 8 * 1024;

        private readonly Socket socket;
        private readonly Dictionary<EndPoint, UDPSocket> instantiatedEndPointSockets;
        private EndPoint remoteEndPoint;

        public bool isBound => this.socket.IsBound;
        public bool isCommunicable { get; private set; }

        public UDPSocket() : this(new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp), null) { }

        private UDPSocket(Socket socket, EndPoint remoteEndPoint) {
            if (socket.SocketType != SocketType.Dgram) { throw new ArgumentException("The socket param MUST be a Dgram socket"); }
            this.socket = socket;
            this.remoteEndPoint = remoteEndPoint;
            this.instantiatedEndPointSockets = new Dictionary<EndPoint, UDPSocket>();
            this.isCommunicable = true;
            this.Configure();
        }

        public void Dispose() {
            this.socket.Dispose();
        }

        public void Close() {
            this.socket.Close();
        }

        public void BindToRemote(NetEndPoint endPoint) {
            this.remoteEndPoint = this.From(endPoint);
            this.isCommunicable = true;
        }

        public void Read(Action<byte[], IUDPSocket> callback) {
            byte[] buffer = new byte[bufferSize];
            EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            this.socket.BeginReceiveFrom(buffer, 0, bufferSize, SocketFlags.None, ref endPoint, asyncResult => {
                var count = this.socket.EndReceiveFrom(asyncResult, ref endPoint);
                byte[] shrinked = new byte[count];
                Copy(buffer, ref shrinked);

                if (this.instantiatedEndPointSockets.TryGetValue(endPoint, out UDPSocket value)) {
                    callback.Invoke(shrinked, value);
                } else {
                    var socket = new UDPSocket(this.socket, endPoint);
                    this.instantiatedEndPointSockets[endPoint] = socket;
                    callback.Invoke(shrinked, socket);
                }
            }, null);
        }

        public void Write(byte[] bytes, Action<int> callback) {
            this.socket.BeginSendTo(bytes, 0, bytes.Length, SocketFlags.None, this.remoteEndPoint, asyncResult => {
                var writtenCount = this.socket.EndSendTo(asyncResult);
                callback.Invoke(writtenCount);
            }, null);
        }

        public override string ToString() {
            return $"{{EndPoint-{this.remoteEndPoint}}}";
        }

        #region Server

        public void Bind(NetEndPoint endPoint) {
            this.socket.Bind(this.From(endPoint));
            this.isCommunicable = true;
        }

        #endregion

        #region Private Methods

        private void Configure() {
            this.socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
        }

        private IPEndPoint From(NetEndPoint ep) {
            return new IPEndPoint(IPAddress.Parse(ep.host), ep.port);
        }

        private static void Copy(byte[] source, ref byte[] destination) {
            for (var i = 0; i < destination.Length; i++) {
                destination[i] = source[i];
            }
        }

        #endregion
    }
}