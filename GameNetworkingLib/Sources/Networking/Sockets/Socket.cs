using System;
using System.Net;
using System.Net.Sockets;
using GameNetworking.Commons;
using Logging;

namespace GameNetworking.Sockets {
    public interface ISocket<TDerived>
        where TDerived : ISocket<TDerived> {
        NetEndPoint localEndPoint { get; }
        NetEndPoint remoteEndPoint { get; }

        bool isConnected { get; }

        void Bind(NetEndPoint endPoint);

        void Connect(NetEndPoint endPoint);

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

    public interface ITcpSocketIOListener<TDerived>
        where TDerived : ITcpSocket<TDerived> {
        void SocketDidReceiveBytes(TDerived socket, byte[] bytes, int count);
        void SocketDidSendBytes(TDerived socket, int count);
    }

    public interface ITcpSocket<TDerived> : ISocket<TDerived>, IEquatable<TDerived>
        where TDerived : ITcpSocket<TDerived> {
        ITcpServerListener<TDerived> serverListener { get; set; }
        ITcpClientListener clientListener { get; set; }

        void Start();
        void Stop();

        void Receive();
        void Send(byte[] bytes, int count);

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

        public ITcpSocketIOListener<TcpSocket> ioListener { get; set; }
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
            this.socket.SendTimeout = 10000;
            this.socket.ReceiveTimeout = 10000;

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
                if (this.socket == null) {
                    this.serverListener?.SocketDidAccept(null);
                    return;
                }

                Socket accepted = this.socket.EndAccept(ar);
                if (accepted == null) {
                    this.serverListener?.SocketDidAccept(null);
                    return;
                }

                var tcpSocket = new TcpSocket(accepted);
                this.serverListener?.SocketDidAccept(tcpSocket);
                tcpSocket.hasBeenConnected = true;
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
            this.socket.BeginConnect(ipep, ConnectResult, new ConnectState { ipEndPoint = ipep });
        }

        struct ConnectState { public IPEndPoint ipEndPoint; }

        private void ConnectResult(IAsyncResult ar) {
            var state = (ConnectState)ar.AsyncState;
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
            this.ipEndPointPool.Pay(state.ipEndPoint);
        }

        public void Disconnect() {
            this.socket?.Disconnect(false);
            this.Close();
            this.clientListener?.SocketDidDisconnect();
        }

        #endregion

        private void CheckClosed() {
            Logger.Log("Checking socket closed. Should be doing that?");
            if (this.isClosed || this.socket == null) { return; }
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

        public void Close() {
            this.CheckClosed();
        }

        #region Read & Write

        public void Receive() {
            if (this.socket == null) { return; }
            var buffer = this.bufferPool.Rent();
            this.socket.BeginReceive(buffer, 0, Consts.bufferSize, SocketFlags.None, ReceiveResult, new ReceiveState { buffer = buffer });
        }

        struct ReceiveState { public byte[] buffer; }

        private void ReceiveResult(IAsyncResult ar) {
            var state = (ReceiveState)ar.AsyncState;
            int count = this.socket.EndReceive(ar);

            if (this.CheckDisconnected()) {
                this.serverListener?.SocketDidDisconnect(this);
                this.clientListener?.SocketDidDisconnect();
                this.CheckClosed();
                return;
            } else {
                this.ioListener?.SocketDidReceiveBytes(this, state.buffer, count);
            }
            this.bufferPool.Pay(state.buffer);
        }

        public void Send(byte[] bytes, int count) {
            if (this.socket == null) { return; }

            this.socket.BeginSend(bytes, 0, count, SocketFlags.None, SendResult, null);
        }

        private void SendResult(IAsyncResult ar) {
            int written = this.socket.EndSend(ar);

            if (this.CheckDisconnected()) {
                this.serverListener?.SocketDidDisconnect(this);
                this.clientListener?.SocketDidDisconnect();
                this.CheckClosed();
                return;
            } else {
                this.ioListener?.SocketDidSendBytes(this, written);
            }
        }

        #endregion

        #region Private Methods

        private bool CheckDisconnected() {
            return this.hasBeenConnected && (!this.isConnected || !this.IsConnected());
        }

        private bool IsConnected() {
            try {
                if (this.socket == null) { return false; }
                return !(this.socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
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

    public interface IUdpSocket<TDerived> : ISocket<TDerived>
        where TDerived : IUdpSocket<TDerived> {
        void Receive();
        void Send(byte[] bytes, int count, NetEndPoint to);
    }

    public interface IUdpSocketIOListener {
        void SocketDidReceiveBytes(UdpSocket socket, byte[] bytes, int count, NetEndPoint from);
        void SocketDidWriteBytes(UdpSocket socket, int count, NetEndPoint to);
    }

    public sealed class UdpSocket : IUdpSocket<UdpSocket> {
        private const int SIO_UDP_CONNRESET = -1744830452;

        private readonly ObjectPool<byte[]> bufferPool;
        private readonly ObjectPool<IPEndPoint> ipEndPointPool;
        private readonly ObjectPool<NetEndPoint> netEndPointPool;
        private Socket socket;

        public bool isBound => this.socket.IsBound;
        public bool isConnected => this.remoteEndPoint != null;

        public NetEndPoint localEndPoint { get; private set; } = new NetEndPoint();
        public NetEndPoint remoteEndPoint { get; private set; } = new NetEndPoint();

        public IUdpSocketIOListener listener { get; set; }

        public UdpSocket() {
            this.bufferPool = new ObjectPool<byte[]>(() => new byte[Consts.bufferSize]);
            this.ipEndPointPool = new ObjectPool<IPEndPoint>(() => new IPEndPoint(IPAddress.Any, 0));
            this.netEndPointPool = new ObjectPool<NetEndPoint>(() => new NetEndPoint());
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
            this.localEndPoint = endPoint;
            var ipep = this.ipEndPointPool.Rent();
            ipep.Address = IPAddress.Parse(endPoint.host);
            ipep.Port = endPoint.port;
            this.Bind(ipep);
            this.ipEndPointPool.Pay(ipep);
        }

        public void Connect(NetEndPoint endPoint) => this.remoteEndPoint = endPoint;

        public void Close() {
            if (this.socket == null) { return; }
            try {
                if (this.socket.Connected) {
                    this.socket.Shutdown(SocketShutdown.Both);
                }
            } finally { this.socket.Close(); }
            this.socket = null;
        }

        public override string ToString() {
            return $"{{EndPoint-{this.remoteEndPoint}}}";
        }

        #region Read & Write

        public void Receive() {
            if (this.socket == null) { return; }

            var buffer = this.bufferPool.Rent();
            var netEndPoint = this.netEndPointPool.Rent();
            var endPoint = this.ipEndPointPool.Rent();
            endPoint.Address = IPAddress.Any;
            endPoint.Port = 0;
            EndPoint ep = endPoint;
            this.socket.BeginReceiveFrom(buffer, 0, Consts.bufferSize, SocketFlags.None, ref ep, ReceiveResult, new ReceiveState {
                buffer = buffer,
                netEndPoint = netEndPoint,
                ipEndPoint = endPoint
            });
        }

        struct ReceiveState {
            public byte[] buffer;
            public NetEndPoint netEndPoint;
            public IPEndPoint ipEndPoint;
        }

        private void ReceiveResult(IAsyncResult ar) {
            var state = (ReceiveState)ar.AsyncState;
            EndPoint ep = state.ipEndPoint;
            var readByteCount = this.socket.EndReceiveFrom(ar, ref ep);

            state.netEndPoint.From(ep);
            this.listener?.SocketDidReceiveBytes(this, state.buffer, readByteCount, state.netEndPoint);

            this.bufferPool.Pay(state.buffer);
            this.ipEndPointPool.Pay(state.ipEndPoint);
            this.netEndPointPool.Pay(state.netEndPoint);
        }

        public void Send(byte[] bytes, int count, NetEndPoint to) {
            if (this.socket == null) { return; }

            IPEndPoint endPoint = this.ipEndPointPool.Rent();
            endPoint.Address = IPAddress.Parse(to.host);
            endPoint.Port = to.port;
            var writtenCount = this.socket.SendTo(bytes, 0, count, SocketFlags.None, endPoint);
            this.listener?.SocketDidWriteBytes(this, writtenCount, to);
            this.ipEndPointPool.Pay(endPoint);
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
    }

    #endregion
}