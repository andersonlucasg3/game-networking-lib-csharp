using System;
using System.Net;
using System.Net.Sockets;

namespace Networking.Sockets {
    using Commons.Models;
    using Commons.Sockets;

    public interface ITCPSocket : ISocket {
        bool isConnected { get; }

        void Close();

        #region Server

        void Listen(int backlog);
        void Accept(Action<ITCPSocket> callback);

        #endregion

        #region Client

        void Connect(NetEndPoint endPoint, Action callback);
        void Disconnect(Action callback);

        void Read(Action<byte[]> callback);

        #endregion
    }

    public sealed class TCPNonBlockingSocket : ITCPSocket, IDisposable {
        private readonly Socket socket;

        public bool isConnected => this.socket.Connected;
        public bool isBound => this.socket.IsBound;

        public bool isCommunicable => this.isConnected;

        public TCPNonBlockingSocket() {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) {
                NoDelay = true,
                Blocking = false,
                SendTimeout = 2000,
                ReceiveTimeout = 2000
            };
        }

        private TCPNonBlockingSocket(Socket socket) {
            this.socket = socket;
            this.socket.NoDelay = true;
            this.socket.Blocking = false;
            this.socket.SendTimeout = 2000;
            this.socket.ReceiveTimeout = 2000;
        }

        public void Dispose() {
            this.socket.Dispose();
        }

        #region Server

        public void Accept(Action<ITCPSocket> acceptAction) {
            this.socket.BeginAccept((ar) => {
                var accepted = this.socket.EndAccept(ar);
                acceptAction?.Invoke(new TCPNonBlockingSocket(accepted));
            }, null);
        }

        public void Bind(NetEndPoint endPoint) {
            this.socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            this.socket.Bind(this.From(endPoint));
        }

        public void Listen(int backlog) {
            this.socket.Listen(backlog);
        }

        #endregion

        #region Client

        public void Connect(NetEndPoint endPoint, Action connectAction) {
            this.socket.BeginConnect(this.From(endPoint), (ar) => {
                this.socket.EndConnect(ar);
                connectAction?.Invoke();
            }, null);
        }

        public void Disconnect(Action disconnectAction) {
            this.socket.BeginDisconnect(false, (ar) => {
                this.socket.EndDisconnect(ar);
                disconnectAction?.Invoke();
            }, null);
        }

        #endregion

        public void Close() {
            try {
                this.socket.Shutdown(SocketShutdown.Both);
            } finally {
                this.socket.Close();
            }
        }

        #region Read & Write

        public void Read(Action<byte[]> readAction) {
            var bufferSize = 1024;
            byte[] buffer = new byte[bufferSize];
            this.socket.BeginReceive(buffer, 0, bufferSize, SocketFlags.Partial, (ar) => {
                var count = this.socket.EndReceive(ar);

                byte[] shrinked = new byte[count];
                this.Copy(buffer, ref shrinked);

                readAction?.Invoke(shrinked);
            }, this);
        }

        public void Write(byte[] bytes, Action<int> writeAction) {
            this.socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.Partial, (ar) => {
                int written = this.socket.EndSend(ar);
                writeAction?.Invoke(written);
            }, this);
        }

        #endregion

        #region Private Methods

        private IPEndPoint From(NetEndPoint ep) {
            return new IPEndPoint(IPAddress.Parse(ep.host), ep.port);
        }

        private void Copy(byte[] source, ref byte[] destination) {
            for (var i = 0; i < destination.Length; i++) {
                destination[i] = source[i];
            }
        }

        #endregion
    }
}
