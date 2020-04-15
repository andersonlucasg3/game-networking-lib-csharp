using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Logging;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace Networking.Sockets {
    public interface IUDPSocket : ISocket, IEquatable<IPEndPoint> {
        void BindToRemote(NetEndPoint endPoint);
        void Unbind();

        void Close();

        void Read(Action<byte[], IUDPSocket> callback);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "<Pending>")]
    public sealed class UDPSocket : IUDPSocket {
        private const int bufferSize = 1024 * 1024;
        private const int SIO_UDP_CONNRESET = -1744830452;
        
        private readonly UDPSocket parent;
        private Socket socket;
        private IPEndPoint boundEndPoint;
        private IPEndPoint remoteEndPoint;

        private readonly Dictionary<EndPoint, UDPSocket> instantiatedEndPointSockets;

        public bool isBound => this.socket.IsBound;
        public bool isCommunicable { get; private set; }

        public UDPSocket() {
            this.instantiatedEndPointSockets = new Dictionary<EndPoint, UDPSocket>();
        }

        private UDPSocket(UDPSocket parent, IPEndPoint remoteEndPoint) : this() {
            this.parent = parent;
            this.socket = parent.socket;

            this.remoteEndPoint = remoteEndPoint;
            this.isCommunicable = true;
        }

        public void Close() {
            this.CleanUpSocket();

            if (this.socket == null) { return; }
            this.socket.Close();
            this.socket = null;
        }

        public void Bind(NetEndPoint endPoint) {
            this.boundEndPoint = this.From(endPoint);
            this.socket = new Socket(this.boundEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp) { DontFragment = true };
            this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            try { this.socket.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null); } catch { Logger.Log($"Error setting SIO_UDP_CONNRESET. Maybe not running on Windows."); }
            this.socket.Bind(this.boundEndPoint);
            this.isCommunicable = true;
        }

        public void BindToRemote(NetEndPoint endPoint) {
            Logger.Log($"Connected to {endPoint}");
            this.remoteEndPoint = this.From(endPoint);
            this.isCommunicable = true;
        }

        public void Unbind() {
            this.CleanUpSocket();
        }

        public void Read(Action<byte[], IUDPSocket> callback) {
            if (this.socket == null) {
                callback.Invoke(null, null);
                return;
            }

            var buffer = new byte[bufferSize];
            EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            this.socket.BeginReceiveFrom(buffer, 0, bufferSize, SocketFlags.None, ref endPoint, ar => {
                if (this.socket == null) { return; }

                var readBytes = this.socket.EndReceiveFrom(ar, ref endPoint);

                if (!this.instantiatedEndPointSockets.TryGetValue(endPoint, out UDPSocket socket)) {
                    socket = new UDPSocket(this, endPoint as IPEndPoint);
                    this.instantiatedEndPointSockets[endPoint] = socket;
                }

                byte[] shrinkedBuffer = new byte[readBytes];
                Array.Copy(buffer, shrinkedBuffer, readBytes);

                callback.Invoke(shrinkedBuffer, socket);
            }, null);
        }

        public void Write(byte[] bytes, Action<int> callback) {
            if (bytes.Length == 0 || this.socket == null) {
                callback.Invoke(0);
                return;
            }

            this.socket.BeginSendTo(bytes, 0, bytes.Length, SocketFlags.None, this.remoteEndPoint, ar => {
                var writtenCount = this.socket.EndSendTo(ar);
                callback.Invoke(writtenCount);
            }, null);
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

        private void CleanUpSocket() {
            if (this.parent == null) { return; }
            this.parent.instantiatedEndPointSockets.Remove(this.remoteEndPoint);
        }

        #endregion
    }
}