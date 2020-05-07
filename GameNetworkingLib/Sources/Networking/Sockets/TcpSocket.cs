using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using GameNetworking.Commons;
using Logging;

namespace GameNetworking.Networking.Sockets {
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

        void Send(byte[] bytes, int count);

        void Disconnect();
    }

    public sealed class TcpSocket : ITcpSocket<TcpSocket> {
        private readonly ObjectPool<byte[]> bufferPool;
        private readonly ObjectPool<IPEndPoint> ipEndPointPool;
        private readonly object closeLock = new object();
        private Socket socket;

        private bool isClosed = false;
        private bool hasBeenConnected = false;

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
            this.socket.ReceiveBufferSize = Consts.bufferSize;
            this.socket.SendBufferSize = Consts.bufferSize;

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

            ThreadPool.QueueUserWorkItem(_ => {
                do { this.Accept(); } while (this.socket != null);
            });
        }

        public void Stop() {
            this.CheckClosed();
        }

        public void Bind(NetEndPoint endPoint) {
            this.localEndPoint = endPoint;

            var ipep = this.ipEndPointPool.Rent();
            this.From(endPoint, ref ipep);
            this.socket.Bind(ipep);
            this.ipEndPointPool.Pay(ipep);
        }

        private void Accept() {
            lock (this.closeLock) {
                if (this.socket == null) { return; }
                if (!this.socket.Poll(-1, SelectMode.SelectRead)) { return; }

                var accepted = this.socket.Accept();
                if (accepted != null) {
                    this.serverListener?.SocketDidAccept(new TcpSocket(accepted) { hasBeenConnected = true });
                }
            }
        }

        #endregion

        #region Client

        struct ConnectState { public IPEndPoint ipEndPoint; }

        public void Connect(NetEndPoint endPoint) {
            this.remoteEndPoint = endPoint;

            var ipep = this.ipEndPointPool.Rent();
            this.From(endPoint, ref ipep);
            this.socket.BeginConnect(ipep, ConnectResult, new ConnectState { ipEndPoint = ipep });
        }

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
            lock (this.closeLock) {
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
        }

        public void Close() {
            this.CheckClosed();
        }

        #region Read & Write

        public void Receive() {
            lock (this.closeLock) {
                if (this.socket == null) { return; }
                if (!this.socket.Poll(-1, SelectMode.SelectRead) || this.socket.Available == 0) { return; }

                if (this.CheckDisconnected()) {
                    this.serverListener?.SocketDidDisconnect(this);
                    this.clientListener?.SocketDidDisconnect();
                    this.CheckClosed();
                    return;
                } else {
                    var buffer = this.bufferPool.Rent();
                    int count = this.socket.Receive(buffer, 0, Consts.bufferSize, SocketFlags.None);
                    this.ioListener?.SocketDidReceiveBytes(this, buffer, count);
                    this.bufferPool.Pay(buffer);
                }
            }
        }

        public void Send(byte[] bytes, int count) {
            lock (this.closeLock) {
                if (this.socket == null) { return; }
                if (!this.socket.Poll(-1, SelectMode.SelectWrite)) { return; }

                if (this.CheckDisconnected()) {
                    this.serverListener?.SocketDidDisconnect(this);
                    this.clientListener?.SocketDidDisconnect();
                    this.CheckClosed();
                } else {
                    int written = this.socket.Send(bytes, 0, count, SocketFlags.None);
                    this.ioListener?.SocketDidSendBytes(this, written);
                }
            }
        }

        #endregion

        #region Private Methods

        private bool CheckDisconnected() {
            return this.hasBeenConnected && !this.isConnected;
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
}