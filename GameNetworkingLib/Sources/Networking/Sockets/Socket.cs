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

    public interface ITcpServerListener {
        void SocketDidAccept(ITcpSocket socket);
        void SocketDidDisconnect(ITcpSocket socket);
    }

    public interface ITcpClientListener {
        void SocketDidConnect();
        void SocketDidTimeout();
        void SocketDidDisconnect();
    }

    public interface ITcpSocket : ISocket {
        NetEndPoint localEndPoint { get; }
        NetEndPoint remoteEndPoint { get; }

        ITcpServerListener serverListener { get; set; }
        ITcpClientListener clientListener { get; set; }

        void Start();
        void Stop();

        void Accept();

        void Disconnect();
    }

    public sealed class TcpSocket : ITcpSocket {
        private readonly ObjectPool<byte[]> bufferPool;
        private readonly ObjectPool<IPEndPoint> ipEndPointPool;

        private bool isClosed = false;

        internal Socket socket;

        public bool isConnected => this.socket.Connected;
        public bool isBound => this.socket.IsBound;

        public NetEndPoint localEndPoint { get; private set; }
        public NetEndPoint remoteEndPoint { get; private set; }

        public ISocketListener listener { get; set; }
        public ITcpServerListener serverListener { get; set; }
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
        }

        #region Server

        public void Start() {
            this.socket.Listen(0);
        }

        public void Stop() {
            try { this.socket.Shutdown(SocketShutdown.Both); } finally { this.socket.Close(); }
        }

        public void Accept() {
            this.socket.BeginAccept((ar) => {
                var accepted = this.socket.EndAccept(ar);
                this.serverListener?.SocketDidAccept(new TcpSocket(accepted));
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
                    this.clientListener?.SocketDidConnect();
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
            if (this.socket == null) { return; }
            try {
                this.socket.Shutdown(SocketShutdown.Both);
            } finally {
                this.socket.Close();
            }
            this.socket = null;
            this.isClosed = true;
        }

        #region Read & Write

        public void Receive() {
            var buffer = this.bufferPool.Rent();

            if (!this.socket.Connected) {
                this.serverListener.SocketDidDisconnect(this);
                this.CheckClosed();
                return;
            }

            this.socket.BeginReceive(buffer, 0, Consts.bufferSize, SocketFlags.None, (ar) => {
                var count = this.socket.EndReceive(ar);
                this.listener?.SocketDidReceiveBytes(buffer, count);
                this.bufferPool.Pay(buffer);
            }, this);
        }

        public void Send(byte[] bytes, int count) {
            this.socket.BeginSend(bytes, 0, count, SocketFlags.None, (ar) => {
                int written = this.socket.EndSend(ar);
                this.listener?.SocketDidSendBytes(written);
            }, this);
        }

        #endregion

        #region Private Methods

        private void From(NetEndPoint ep, ref IPEndPoint endPoint) {
            endPoint.Address = IPAddress.Parse(ep.host);
            endPoint.Port = ep.port;
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

        public NetEndPoint remoteEndPoint { get; private set; }

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
            if (this.socket == null) {
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
            if (bytes.Length == 0 || this.socket == null) {
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