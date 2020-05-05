using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using GameNetworking.Commons;
using Logging;

namespace GameNetworking.Sockets {
    public interface ISocketListener<TSocket>
        where TSocket : ISocket<TSocket> {
        void SocketDidReceiveBytes(TSocket socket, byte[] bytes, int count);
        void SocketDidSendBytes(TSocket socket, int count);
    }

    public interface ISocket<TDerived>
        where TDerived : ISocket<TDerived> {
        NetEndPoint localEndPoint { get; }
        NetEndPoint remoteEndPoint { get; }

        bool isConnected { get; }

        ISocketListener<TDerived> listener { get; set; }

        void Bind(NetEndPoint endPoint);

        void Connect(NetEndPoint endPoint);

        void Receive();
        void Send(byte[] bytes, int count);

        void Close();
    }

    #region TCP

    public interface ITcpServerListener<TDerived>
        where TDerived : ITcpSocket<TDerived> {
        void SocketDidAccept(TDerived socket);
        void SocketDidDisconnect(TDerived socket);
    }

    public interface ITcpClientListener {
        void SocketDidConnect();
        void SocketDidTimeout();
        void SocketDidDisconnect();
    }

    public interface ITcpSocket<TDerived> : ISocket<TDerived>, IEquatable<TDerived>
        where TDerived : ITcpSocket<TDerived> {
        ITcpServerListener<TDerived> serverListener { get; set; }
        ITcpClientListener clientListener { get; set; }

        void Start();
        void Stop();

        void Accept();

        void Disconnect();
    }

    public sealed class TcpSocket : ITcpSocket<TcpSocket> {
        private readonly ObjectPool<byte[]> bufferPool;
        private readonly ObjectPool<IPEndPoint> ipEndPointPool;
        private readonly Object sendLockCookie = new Object();
        private readonly Object receiveLockCookie = new Object();

        private bool isClosed = false;
        private bool hasBeenConnected = false;

        internal Socket socket;

        public bool isConnected => this.socket?.Connected ?? false;
        public bool isBound => this.socket?.IsBound ?? false;

        public NetEndPoint localEndPoint { get; private set; } = new NetEndPoint();
        public NetEndPoint remoteEndPoint { get; private set; } = new NetEndPoint();

        public ISocketListener<TcpSocket> listener { get; set; }
        public ITcpServerListener<TcpSocket> serverListener { get; set; }
        public ITcpClientListener clientListener { get; set; }

        public TcpSocket() : this(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) { }

        private TcpSocket(Socket socket) {
            this.bufferPool = new ObjectPool<byte[]>(() => new byte[Consts.bufferSize]);
            this.ipEndPointPool = new ObjectPool<IPEndPoint>(() => new IPEndPoint(IPAddress.Any, 0));
            this.socket = socket;

            this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            this.socket.NoDelay = true;
            this.socket.Blocking = false;
            this.socket.SendTimeout = 2000;
            this.socket.ReceiveTimeout = 2000;

            if (this.socket.LocalEndPoint != null) {
                this.localEndPoint.From(this.socket.LocalEndPoint);
            }
            if (this.socket.RemoteEndPoint != null) {
                this.remoteEndPoint.From(this.socket.RemoteEndPoint);
            }
        }

        #region Server

        public void Start() {
            this.socket.Listen(0);
        }

        public void Stop() {
            this.CheckClosed();
        }

        public void Accept() {
            this.socket.BeginAccept((ar) => {
                Socket accepted = null;
                lock (this) {
                    if (this.socket != null) {
                        accepted = this.socket.EndAccept(ar);
                    }
                }

                if (accepted == null) {
                    this.serverListener?.SocketDidAccept(null);
                    return;
                }

                var tcpSocket = new TcpSocket(accepted) { hasBeenConnected = true };
                this.serverListener?.SocketDidAccept(tcpSocket);
            }, null);
        }

        public void Bind(NetEndPoint endPoint) {
            this.localEndPoint = endPoint;

            var ipep = this.ipEndPointPool.Rent();
            this.From(endPoint, ref ipep);
            this.socket.Bind(ipep);
            this.ipEndPointPool.Pay(ipep);
        }

        #endregion

        #region Client

        public void Connect(NetEndPoint endPoint) {
            this.remoteEndPoint = endPoint;

            var ipep = this.ipEndPointPool.Rent();
            this.From(endPoint, ref ipep);
            this.socket.BeginConnect(ipep, (ar) => {
                if (this.socket.Connected) {
                    this.socket.EndConnect(ar);
                    this.localEndPoint.From(this.socket.LocalEndPoint);
                    this.remoteEndPoint.From(this.socket.RemoteEndPoint);
                    this.hasBeenConnected = true;
                    this.clientListener?.SocketDidConnect();
                } else {
                    this.clientListener?.SocketDidTimeout();
                    this.CheckClosed();
                }
                this.ipEndPointPool.Pay(ipep);
            }, null);
        }

        public void Disconnect() {
            this.socket?.Disconnect(false);
            this.Close();
            this.clientListener?.SocketDidDisconnect();
        }

        #endregion

        private void CheckClosed() {
            if (this.isClosed) { return; }
            lock (this) {
                try {
                    if (!this.CheckDisconnected()) {
                        this.socket.Shutdown(SocketShutdown.Both);
                    }
                } finally {
                    this.socket.Close();
                }
                this.socket = null;
                this.isClosed = true;
                this.hasBeenConnected = false;
            }
        }

        public void Close() {
            this.CheckClosed();
        }

        #region Read & Write

        public void Receive() {
            lock (this.receiveLockCookie) {
                if (this.socket == null) { return; }
                var buffer = this.bufferPool.Rent();
                int count = this.socket.Receive(buffer, 0, Consts.bufferSize, SocketFlags.None, out SocketError errorCode);

                this.listener?.SocketDidReceiveBytes(this, buffer, count);
                this.bufferPool.Pay(buffer);

                if (this.CheckDisconnected() || this.CheckSocketError(errorCode)) {
                    this.serverListener?.SocketDidDisconnect(this);
                    this.clientListener?.SocketDidDisconnect();
                    this.CheckClosed();
                    return;
                }
            }
        }

        public void Send(byte[] bytes, int count) {
            lock (this.sendLockCookie) {
                int written = this.socket.Send(bytes, 0, count, SocketFlags.None, out SocketError errorCode);

                if (this.CheckDisconnected() || this.CheckSocketError(errorCode)) {
                    this.serverListener?.SocketDidDisconnect(this);
                    this.clientListener?.SocketDidDisconnect();
                    this.CheckClosed();
                    return;
                } else {
                    this.listener?.SocketDidSendBytes(this, written);
                }
            }
        }

        #endregion

        #region Private Methods

        private bool CheckDisconnected() {
            return this.hasBeenConnected && !this.isConnected;
        }

        private bool CheckSocketError(SocketError error) {
            switch (error) {
                case SocketError.Shutdown:
                case SocketError.OperationAborted:
                case SocketError.NotConnected:
                case SocketError.NetworkUnreachable:
                case SocketError.NetworkReset:
                case SocketError.NetworkDown:
                case SocketError.Interrupted:
                case SocketError.HostUnreachable:
                case SocketError.HostNotFound:
                case SocketError.HostDown:
                case SocketError.Fault:
                case SocketError.Disconnecting:
                case SocketError.ConnectionReset:
                case SocketError.ConnectionRefused:
                case SocketError.ConnectionAborted:
                    return true;
                default:
                    return false;
            }
        }

        private void From(NetEndPoint ep, ref IPEndPoint endPoint) {
            endPoint.Address = IPAddress.Parse(ep.host);
            endPoint.Port = ep.port;
        }

        #endregion

        #region IEquatable

        bool IEquatable<TcpSocket>.Equals(TcpSocket other) {
            return this.remoteEndPoint.Equals(other.remoteEndPoint);
        }

        #endregion
    }

    #endregion

    #region UDP

    public interface IUdpSocket<TDerived> : ISocket<TDerived>
        where TDerived : IUdpSocket<TDerived> { }

    public sealed class UdpSocket : IUdpSocket<UdpSocket> {
        private const int SIO_UDP_CONNRESET = -1744830452;

        private readonly ObjectPool<byte[]> bufferPool;
        private readonly ObjectPool<IPEndPoint> ipEndPointPool;
        private Socket socket;

        public bool isBound => this.socket.IsBound;
        public bool isConnected => this.remoteEndPoint != null;

        public NetEndPoint localEndPoint { get; private set; } = new NetEndPoint();
        public NetEndPoint remoteEndPoint { get; private set; } = new NetEndPoint();

        public ISocketListener<UdpSocket> listener { get; set; }

        public UdpSocket() {
            this.bufferPool = new ObjectPool<byte[]>(() => new byte[Consts.bufferSize]);
            this.ipEndPointPool = new ObjectPool<IPEndPoint>(() => new IPEndPoint(IPAddress.Any, 0));
        }

        public UdpSocket(UdpSocket socket, NetEndPoint remoteEndPoint) : this() {
            this.socket = socket.socket;
            this.remoteEndPoint = remoteEndPoint;
        }

        private void Bind(IPEndPoint endPoint) {
            this.socket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp) {
                ReceiveBufferSize = Consts.bufferSize,
                SendBufferSize = Consts.bufferSize
            };
            this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            try { this.socket.DontFragment = true; } catch (Exception) { Logger.Log("DontFragment not supported."); }
            try { this.socket.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null); } catch (Exception) { Logger.Log("Error setting SIO_UDP_CONNRESET. Maybe not running on Windows."); }
            this.socket.Bind(endPoint);

            Logger.Log($"Listening on {endPoint}");
        }

        public void Bind(NetEndPoint endPoint) {
            var ipep = this.ipEndPointPool.Rent();
            this.From(endPoint, ref ipep);
            this.Bind(ipep);
            this.ipEndPointPool.Pay(ipep);
            this.localEndPoint = endPoint;
        }

        public void Connect(NetEndPoint endPoint) => this.remoteEndPoint = endPoint;

        public void Close() {
            if (this.socket == null) { return; }
            try { this.socket.Shutdown(SocketShutdown.Both); } finally { this.socket.Close(); }
            this.socket = null;
        }

        public override string ToString() {
            return $"{{EndPoint-{this.remoteEndPoint}}}";
        }

        #region Read & Write

        public void Receive() {
            var buffer = this.bufferPool.Rent();
            EndPoint endPoint = this.ipEndPointPool.Rent();
            var readByteCount = this.socket.ReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endPoint);

            this.remoteEndPoint.From(endPoint);
            this.listener?.SocketDidReceiveBytes(this, buffer, readByteCount);

            this.bufferPool.Pay(buffer);
            this.ipEndPointPool.Pay((IPEndPoint)endPoint);
        }

        public void Send(byte[] bytes, int count) {
            if (count == 0 || this.socket == null || !this.isConnected) {
                this.listener?.SocketDidSendBytes(this, 0);
                return;
            }

            var endPoint = this.ipEndPointPool.Rent();
            this.From(this.remoteEndPoint, ref endPoint);

            var writtenCount = this.socket.SendTo(bytes, 0, count, SocketFlags.None, endPoint);

            this.ipEndPointPool.Pay(endPoint);

            this.listener?.SocketDidSendBytes(this, writtenCount);
        }

        #endregion

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

        private void From(NetEndPoint ep, ref IPEndPoint endPoint) {
            endPoint.Address = IPAddress.Parse(ep.host);
            endPoint.Port = ep.port;
        }

        #endregion

        private struct SendInfo {
            public byte[] bytes;
            public int count;
        }
    }

    #endregion
}