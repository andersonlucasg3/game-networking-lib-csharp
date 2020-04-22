using System;
using System.Net;
using System.Net.Sockets;
using GameNetworking.Commons;
using Logging;

namespace GameNetworking.Sockets {
    public interface ISocketListener {
        void SocketDidReceiveBytes(byte[] bytes, int count);
        void SocketDidSendBytes(int count);
    }

    public interface ISocket {
        bool isConnected { get; }

        ISocketListener listener { get; set; }

        void Bind(NetEndPoint endPoint);

        void Connect(NetEndPoint endPoint);

        void Send(byte[] bytes, int count);
        void Receive();

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

    public interface ITcpSocket<TDerived> : ISocket, IEquatable<TDerived>
        where TDerived : ITcpSocket<TDerived> {
        NetEndPoint localEndPoint { get; }
        NetEndPoint remoteEndPoint { get; }

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

        private bool isClosed = false;
        private bool hasBeenConnected = false;

        internal Socket socket;

        public bool isConnected => this.socket?.Connected ?? false;
        public bool isBound => this.socket?.IsBound ?? false;

        public NetEndPoint localEndPoint { get; private set; } = new NetEndPoint();
        public NetEndPoint remoteEndPoint { get; private set; } = new NetEndPoint();

        public ISocketListener listener { get; set; }
        public ITcpServerListener<TcpSocket> serverListener { get; set; }
        public ITcpClientListener clientListener { get; set; }

        public TcpSocket() : this(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) { }

        private TcpSocket(Socket socket) {
            this.bufferPool = new ObjectPool<byte[]>(() => new byte[Consts.bufferSize]);
            this.ipEndPointPool = new ObjectPool<IPEndPoint>(() => new IPEndPoint(IPAddress.Any, 0));
            this.socket = socket;
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

                var socket = new TcpSocket(accepted) { hasBeenConnected = true };
                this.serverListener?.SocketDidAccept(socket);
            }, null);
        }

        public void Bind(NetEndPoint endPoint) {
            this.localEndPoint = endPoint;

            this.socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);

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
                    this.clientListener?.SocketDidConnect();
                    this.hasBeenConnected = true;
                } else {
                    this.clientListener?.SocketDidTimeout();
                    this.CheckClosed();
                }
                this.ipEndPointPool.Pay(ipep);
            }, null);
        }

        public void Disconnect() {
            this.socket.BeginDisconnect(false, (ar) => {
                this.socket.EndDisconnect(ar);
                this.clientListener?.SocketDidDisconnect();
            }, null);
        }

        #endregion

        private void CheckClosed() {
            if (!this.isClosed) { this.Close(); }
        }

        public void Close() {
            lock (this) {
                if (this.socket == null) { return; }
                try {
                    if (this.isConnected) {
                        this.socket.Shutdown(SocketShutdown.Both);
                    }
                } finally {
                    this.socket.Close();
                }
                this.socket = null;
                this.isClosed = true;
            }
            this.hasBeenConnected = false;
        }

        #region Read & Write

        public void Receive() {
            if (this.CheckDisconnected()) {
                this.serverListener?.SocketDidDisconnect(this);
                this.clientListener?.SocketDidDisconnect();
                this.CheckClosed();
                return;
            }

            if (!this.isConnected) {
                this.listener?.SocketDidReceiveBytes(null, 0);
                return;
            }

            var buffer = this.bufferPool.Rent();
            this.socket.BeginReceive(buffer, 0, Consts.bufferSize, SocketFlags.None, (ar) => {
                int count = 0;
                lock (this) {
                    if (this.socket != null) {
                        count = this.socket.EndReceive(ar);
                    }
                }
                this.listener?.SocketDidReceiveBytes(buffer, count);
                this.bufferPool.Pay(buffer);
            }, this);
        }

        public void Send(byte[] bytes, int count) {
            if (this.CheckDisconnected()) {
                this.serverListener?.SocketDidDisconnect(this);
                this.clientListener?.SocketDidDisconnect();
                this.CheckClosed();
                return;
            }

            if (!this.isConnected) {
                this.listener?.SocketDidSendBytes(0);
                return;
            }

            this.socket.BeginSend(bytes, 0, count, SocketFlags.None, (ar) => {
                int written = 0;
                lock (this) {
                    if (this.socket != null) {
                        written = this.socket.EndSend(ar);
                    }
                }
                this.listener?.SocketDidSendBytes(written);
            }, this);
        }

        #endregion

        #region Private Methods

        private bool CheckDisconnected() {
            return this.hasBeenConnected && (!this.isConnected || !this.IsConnected());
        }

        private bool IsConnected() {
            try {
                lock (this) {
                    if (this.socket == null) { return false; }
                    return !(this.socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
                }
            } catch (SocketException) { return false; }
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

    public interface IUdpSocketListener {
        void UdpSocketDidReceiveBytes(byte[] bytes, int count, NetEndPoint from);
    }

    public interface IUdpSocket : ISocket {
        NetEndPoint remoteEndPoint { get; }

        IUdpSocketListener udpListener { get; set; }
    }

    public sealed class UdpSocket : IUdpSocket {
        private const int SIO_UDP_CONNRESET = -1744830452;

        private readonly ObjectPool<byte[]> bufferPool;
        private readonly ObjectPool<NetEndPoint> endPointPool;
        private readonly ObjectPool<IPEndPoint> ipEndPointPool;
        private Socket socket;

        public bool isBound => this.socket.IsBound;
        public bool isConnected => this.remoteEndPoint != null;

        public NetEndPoint remoteEndPoint { get; private set; } = new NetEndPoint();

        public ISocketListener listener { get; set; }
        public IUdpSocketListener udpListener { get; set; }

        public UdpSocket() {
            this.bufferPool = new ObjectPool<byte[]>(() => new byte[Consts.bufferSize]);
            this.endPointPool = new ObjectPool<NetEndPoint>(() => new NetEndPoint());
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
        }

        public void Bind(NetEndPoint endPoint) {
            var ipep = this.ipEndPointPool.Rent();
            this.From(endPoint, ref ipep);
            this.Bind(ipep);
            this.ipEndPointPool.Pay(ipep);
        }

        public void Connect(NetEndPoint endPoint) => this.remoteEndPoint = endPoint;

        public void Close() {
            if (this.socket == null) { return; }
            try { this.socket.Shutdown(SocketShutdown.Both); } finally { this.socket.Close(); }
            this.socket = null;
        }

        public void Receive() {
            if (this.socket == null || !this.isBound) {
                this.listener?.SocketDidReceiveBytes(null, 0);
                return;
            }

            var buffer = this.bufferPool.Rent();
            EndPoint endPoint = this.ipEndPointPool.Rent();
            var netEndPoint = this.endPointPool.Rent();
            this.socket.BeginReceiveFrom(buffer, 0, Consts.bufferSize, SocketFlags.None, ref endPoint, ar => {
                var readBytes = this.socket.EndReceiveFrom(ar, ref endPoint);
                netEndPoint.From(endPoint);

                this.udpListener?.UdpSocketDidReceiveBytes(buffer, readBytes, netEndPoint);

                this.bufferPool.Pay(buffer);
                this.ipEndPointPool.Pay((IPEndPoint)endPoint);
                this.endPointPool.Pay(netEndPoint);
            }, null);
        }

        public void Send(byte[] bytes, int count) {
            if (count == 0 || this.socket == null || !this.isConnected) {
                this.listener?.SocketDidSendBytes(0);
                return;
            }

            var endPoint = this.ipEndPointPool.Rent();
            this.From(this.remoteEndPoint, ref endPoint);
            this.socket.BeginSendTo(bytes, 0, count, SocketFlags.None, endPoint, ar => {
                var writtenCount = this.socket.EndSendTo(ar);
                this.listener?.SocketDidSendBytes(writtenCount);
                this.ipEndPointPool.Pay(endPoint);
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

        private void From(NetEndPoint ep, ref IPEndPoint endPoint) {
            endPoint.Address = IPAddress.Parse(ep.host);
            endPoint.Port = ep.port;
        }

        #endregion
    }

    #endregion
}