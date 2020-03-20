using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Networking.IO;
using Networking.Models;

namespace Networking.Sockets {
    public sealed class UDPSocket : ISocket, IDisposable {
        private readonly BaseUDPSocket socket;
        
        public bool isConnected => this.socket.isConnected;
        public bool isBound => this.socket.isBound;

        public ISocket.IListener listener { get; set; }

        public UDPSocket() {
            
        }

        private UDPSocket(BaseUDPSocket socket) {
            this.socket = socket;
        }

        public void Dispose() {
            this.socket.Dispose();
        }

        #region Server

        public void Accept() { /* not applicable */ }

        public void Bind(NetEndPoint endPoint) {
            this.socket.Bind(this.From(endPoint));
        }

        public void Listen(int backlog) { /* not applicable */ }

        #endregion

        public void Close() {
            this.socket.Close();
        }

        public void Connect(NetEndPoint endPoint) {
            this.socket.Connect(this.From(endPoint));
        }

        public void Disconnect() { /* not applicable yet */ }

        public void Read() {
            this.socket.Read(DidRead);
        }

        public void Write(byte[] bytes) {
            // TODO: 
        }

        #region Private Methods

        private IPEndPoint From(NetEndPoint ep) {
            return new IPEndPoint(IPAddress.Parse(ep.host), ep.port);
        }

        private void DidRead(byte[] bytes) {
            this.listener?.SocketDidRead(bytes);
        }

        #endregion
    }

    abstract class BaseUDPSocket : IDisposable {
        class State {
            public byte[] buffer = new byte[bufferSize];
            public EndPoint endPointFrom = new IPEndPoint(IPAddress.Any, 0);
        }

        private readonly Socket socket;
        private const int bufferSize = 8 * 1024;
        private EndPoint sendToEndPoint;

        public bool isConnected => IsConnected();
        public bool isBound => this.socket.IsBound;

        public BaseUDPSocket() {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) {
                SendTimeout = 2000,
                ReceiveTimeout = 2000
            };
            this.Configure();
        }

        public BaseUDPSocket(Socket socket) {
            if (socket.SocketType != SocketType.Dgram) { throw new ArgumentException("The socket param MUST be a Dgram socket"); }
            this.socket = socket;
            this.Configure();
        }

        public void Dispose() {
            if (socket == null) { return; }
            this.socket.Dispose();
        }

        #region Public methods

        public void Connect(EndPoint endPoint) {
            this.sendToEndPoint = endPoint;
            this.socket.Connect(endPoint);
        }

        public void Read(Action<byte[]> callback) {
            var state = new State();
            byte[] buffer = new byte[bufferSize];
            this.socket.BeginReceiveFrom(state.buffer, 0, bufferSize, SocketFlags.None, ref state.endPointFrom, asyncResult => {
                var count = this.socket.EndReceiveFrom(asyncResult, ref state.endPointFrom);
                byte[] shrinked = new byte[count];
                Copy(buffer, ref shrinked);
                callback.Invoke(shrinked);
            }, state);
        }

        public void Write(byte[] bytes, Action<int> callback) {
            this.socket.BeginSendTo(bytes, 0, bytes.Length, SocketFlags.None, this.sendToEndPoint, (ar) => {
                int written = this.socket.EndSend(ar);
                callback.Invoke(written);
            }, this);
        }

        #endregion

        #region Private methods

        private void Configure() {
            this.socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            this.socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
        }

        private static void Copy(byte[] source, ref byte[] destination) {
            for (var i = 0; i < destination.Length; i++) {
                destination[i] = source[i];
            }
        }

        #endregion

        #region Abstract methods

        protected abstract bool IsConnected();

        #endregion
    }
}