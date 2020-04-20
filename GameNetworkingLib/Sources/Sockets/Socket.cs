using System;
using System.Net;
using System.Net.Sockets;
using GameNetworking.Commons;
using Logging;

namespace GameNetworking.Sockets {
    public interface ISocketListener {
        void SocketDidReadBytes(byte[] bytes, int count);
        void SocketDidWriteBytes(int count);
    }

    public interface ISocket {
        bool isConnected { get; }

        ISocketListener listener { get; set; }

        void Bind(NetEndPoint endPoint);

        void Connect(NetEndPoint endPoint);

        void Send(byte[] bytes, int count);
        void Receive();
    }

    #region TCP

    public interface ITcpSocketListener {
        void SocketDidConnect();
        void SocketDidTimeout();
        void SocketDidDisconnect();

        void SocketDidAccept(TcpSocket socket);
    }

    public interface ITCPSocket : ISocket {
        ITcpSocketListener tcpListener { get; set; }

        void Accept();

        void Disconnect();
    }

    public sealed class TcpSocket : ITCPSocket {
        private readonly Socket socket;
        private readonly ObjectPool<byte[]> bufferPool;

        public bool isConnected => this.socket.Connected;
        public bool isBound => this.socket.IsBound;

        public ISocketListener listener { get; set; }
        public ITcpSocketListener tcpListener { get; set; }

        public TcpSocket() {
            this.bufferPool = new ObjectPool<byte[]>(() => new byte[Consts.bufferSize]);
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) {
                NoDelay = true,
                Blocking = false,
                SendTimeout = 2000,
                ReceiveTimeout = 2000
            };
        }

        private TcpSocket(Socket socket) {
            this.socket = socket;
            this.socket.NoDelay = true;
            this.socket.Blocking = false;
            this.socket.SendTimeout = 2000;
            this.socket.ReceiveTimeout = 2000;
        }

        #region Server

        public void Accept() {
            this.socket.BeginAccept((ar) => {
                var accepted = this.socket.EndAccept(ar);
                this.tcpListener?.SocketDidAccept(new TcpSocket(accepted));
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

        public void Connect(NetEndPoint endPoint) {
            this.socket.BeginConnect(this.From(endPoint), (ar) => {
                this.socket.EndConnect(ar);
                this.tcpListener?.SocketDidConnect();
            }, null);
        }

        public void Disconnect() {
            this.socket.BeginDisconnect(false, (ar) => {
                this.socket.EndDisconnect(ar);
                this.tcpListener?.SocketDidDisconnect();
            }, null);
        }

        #endregion

        public void Close() {
            try { this.socket.Shutdown(SocketShutdown.Both); } finally { this.socket.Close(); }
        }

        #region Read & Write

        public void Receive() {
            var buffer = this.bufferPool.Rent();

            this.socket.BeginReceive(buffer, 0, Consts.bufferSize, SocketFlags.None, (ar) => {
                var count = this.socket.EndReceive(ar);
                this.listener?.SocketDidReadBytes(buffer, count);
                this.bufferPool.Pay(buffer);
            }, this);
        }

        public void Send(byte[] bytes, int count) {
            this.socket.BeginSend(bytes, 0, count, SocketFlags.None, (ar) => {
                int written = this.socket.EndSend(ar);
                this.listener?.SocketDidWriteBytes(written);
            }, this);
        }

        #endregion

        #region Private Methods

        private IPEndPoint From(NetEndPoint ep) => new IPEndPoint(IPAddress.Parse(ep.host), ep.port);

        #endregion
    }

    #endregion

    #region UDP

    public sealed class UdpSocket : ISocket {
        private const int SIO_UDP_CONNRESET = -1744830452;

        private readonly ObjectPool<byte[]> bufferPool;
        private Socket socket;
        private IPEndPoint remoteEndPoint;

        public bool isBound => this.socket.IsBound;
        public bool isConnected => this.remoteEndPoint != null;

        public ISocketListener listener { get; set; }

        public UdpSocket() => this.bufferPool = new ObjectPool<byte[]>(NewBuffer);

        private byte[] NewBuffer() => new byte[Consts.bufferSize];

        public void Bind(NetEndPoint endPoint) {
            var boundEndPoint = this.From(endPoint);
            this.socket = new Socket(boundEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp) {
                ReceiveBufferSize = Consts.bufferSize,
                SendBufferSize = Consts.bufferSize
            };
            this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            try { this.socket.DontFragment = true; } catch (Exception) { Logger.Log("DontFragment not supported."); }
            try { this.socket.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null); } catch (Exception) { Logger.Log("Error setting SIO_UDP_CONNRESET. Maybe not running on Windows."); }
            this.socket.Bind(boundEndPoint);
        }

        public void Connect(NetEndPoint endPoint) {
            Logger.Log($"Connected to {endPoint}");
            this.remoteEndPoint = this.From(endPoint);
        }

        public void Close() {
            if (this.socket == null) { return; }
            try { this.socket.Shutdown(SocketShutdown.Both); } finally { this.socket.Close(); }
            this.socket = null;
        }

        public void Receive() {
            if (this.socket == null) {
                this.listener?.SocketDidReadBytes(null, 0);
                return;
            }

            var buffer = this.bufferPool.Rent();
            EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            this.socket.BeginReceiveFrom(buffer, 0, Consts.bufferSize, SocketFlags.None, ref endPoint, ar => {
                var readBytes = this.socket.EndReceiveFrom(ar, ref endPoint);
                this.listener?.SocketDidReadBytes(buffer, readBytes);
                this.bufferPool.Pay(buffer);
            }, null);
        }

        public void Send(byte[] bytes, int count) {
            if (bytes.Length == 0 || this.socket == null) {
                this.listener?.SocketDidWriteBytes(0);
                return;
            }

            this.socket.BeginSendTo(bytes, 0, count, SocketFlags.None, this.remoteEndPoint, ar => {
                var writtenCount = this.socket.EndSendTo(ar);
                this.listener?.SocketDidWriteBytes(writtenCount);
            }, null);
        }

        public override string ToString() {
            return $"{{EndPoint-{this.remoteEndPoint}}}";
        }

        #region Equatable Methods

        public bool Equals(IPEndPoint endPoint) => this.remoteEndPoint.Equals(endPoint);

        public override bool Equals(object obj) {
            if (obj is UdpSocket other) {
                return this.Equals(other.remoteEndPoint);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode() => this.remoteEndPoint.GetHashCode();

        #endregion

        #region Private Methods

        private IPEndPoint From(NetEndPoint ep) => new IPEndPoint(IPAddress.Parse(ep.host), ep.port);

        #endregion
    }

    #endregion
}