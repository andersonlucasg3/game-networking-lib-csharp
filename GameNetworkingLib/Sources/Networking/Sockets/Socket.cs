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
        private bool isClosed = false;

        internal Socket socket;

        public bool isConnected => this.socket.Connected;
        public bool isBound => this.socket.IsBound;

        public NetEndPoint localEndPoint { get; private set; }
        public NetEndPoint remoteEndPoint { get; private set; }

        public ISocketListener listener { get; set; }
        public ITcpServerListener serverListener { get; set; }
        public ITcpClientListener clientListener { get; set; }

        public TcpSocket() {
            this.bufferPool = new ObjectPool<byte[]>(() => new byte[Consts.bufferSize]);
            this.NewSocket();
        }

        private TcpSocket(Socket socket) {
            this.bufferPool = new ObjectPool<byte[]>(() => new byte[Consts.bufferSize]);
            this.socket = socket;
            this.socket.NoDelay = true;
            this.socket.Blocking = false;
            this.socket.SendTimeout = 2000;
            this.socket.ReceiveTimeout = 2000;
        }

        private void NewSocket() {
            if (this.socket == null) { return; }
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) {
                NoDelay = true,
                Blocking = false,
                SendTimeout = 2000,
                ReceiveTimeout = 2000
            };
        }

        #region Server

        public void Start() {
            this.NewSocket();
            this.socket.Listen(0);
        }

        public void Stop() {
            try { this.socket.Shutdown(SocketShutdown.Both); }
            finally { this.socket.Close(); }
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
            this.socket.Bind(this.From(endPoint));
        }

        #endregion

        #region Client

        public void Connect(NetEndPoint endPoint) {
            this.NewSocket();

            this.remoteEndPoint = endPoint;

            this.socket.BeginConnect(this.From(endPoint), (ar) => {
                if (this.socket.Connected) {
                    this.socket.EndConnect(ar);
                    this.clientListener?.SocketDidConnect();
                } else {
                    this.clientListener?.SocketDidTimeout();
                    this.CheckClosed();
                }
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

        public void Bind(NetEndPoint endPoint) => this.Bind(this.From(endPoint));

        private void Connect(IPEndPoint endPoint) {
            Logger.Log($"Connected to {endPoint}");
            this.remoteEndPoint = endPoint;
        }

        public void Connect(NetEndPoint endPoint) => this.Connect(this.From(endPoint));

        public void Close() {
            if (this.socket == null) { return; }
            try { this.socket.Shutdown(SocketShutdown.Both); }
            finally { this.socket.Close(); }
            this.socket = null;
        }

        public void Receive() {
            if (this.socket == null) {
                this.listener?.SocketDidReceiveBytes(null, 0);
                return;
            }

            var buffer = this.bufferPool.Rent();
            EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            this.socket.BeginReceiveFrom(buffer, 0, Consts.bufferSize, SocketFlags.None, ref endPoint, ar => {
                var readBytes = this.socket.EndReceiveFrom(ar, ref endPoint);
                this.listener?.SocketDidReceiveBytes(buffer, readBytes);
                this.bufferPool.Pay(buffer);
            }, null);
        }

        public void Send(byte[] bytes, int count) {
            if (bytes.Length == 0 || this.socket == null) {
                this.listener?.SocketDidSendBytes(0);
                return;
            }

            this.socket.BeginSendTo(bytes, 0, count, SocketFlags.None, this.remoteEndPoint, ar => {
                var writtenCount = this.socket.EndSendTo(ar);
                this.listener?.SocketDidSendBytes(writtenCount);
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