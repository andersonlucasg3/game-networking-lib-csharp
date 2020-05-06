using System;
using System.Net;
using System.Net.Sockets;
using GameNetworking.Commons;
using Logging;

namespace GameNetworking.Networking.Sockets {
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
        private readonly object closeLock = new object();
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
                SendBufferSize = Consts.bufferSize,
                Blocking = false
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
            lock (this.closeLock) {
                if (this.socket == null) { return; }
                try {
                    if (this.socket.Connected) {
                        this.socket.Shutdown(SocketShutdown.Both);
                    }
                } finally { this.socket.Close(); }
                this.socket = null;
            }
        }

        public override string ToString() {
            return $"{{EndPoint-{this.remoteEndPoint}}}";
        }

        #region Read & Write

        public void Receive() {
            lock (this.closeLock) {
                if (this.socket == null) { return; }

                var buffer = this.bufferPool.Rent();
                var netEndPoint = this.netEndPointPool.Rent();
                var endPoint = this.ipEndPointPool.Rent();
                endPoint.Address = IPAddress.Any;
                endPoint.Port = 0;
                EndPoint ep = endPoint;

                var readByteCount = this.socket.ReceiveFrom(buffer, 0, Consts.bufferSize, SocketFlags.None, ref ep);
                netEndPoint.From(ep);
                this.listener?.SocketDidReceiveBytes(this, buffer, readByteCount, netEndPoint);

                this.bufferPool.Pay(buffer);
                this.ipEndPointPool.Pay(endPoint);
                this.netEndPointPool.Pay(netEndPoint);
            }
        }

        public void Send(byte[] bytes, int count, NetEndPoint to) {
            lock (this.closeLock) {
                if (this.socket == null) { return; }

                IPEndPoint endPoint = this.ipEndPointPool.Rent();
                endPoint.Address = IPAddress.Parse(to.host);
                endPoint.Port = to.port;
                var writtenCount = this.socket.SendTo(bytes, 0, count, SocketFlags.None, endPoint);
                this.listener?.SocketDidWriteBytes(this, writtenCount, to);
                this.ipEndPointPool.Pay(endPoint);
            }
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
}