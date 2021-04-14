using System;
using System.Net;
using System.Net.Sockets;
using GameNetworking.Commons;
using Logging;

namespace GameNetworking.Networking.Sockets
{
    public interface IUdpSocket : ISocket
    {
        void Receive();
        void Send(byte[] bytes, int count, NetEndPoint to);
    }

    public interface IUdpSocketIOListener
    {
        void SocketDidReceiveBytes(UdpSocket socket, byte[] bytes, int count, NetEndPoint from);
        void SocketDidWriteBytes(UdpSocket socket, int count, NetEndPoint to);
    }

    public sealed class UdpSocket : IUdpSocket
    {
        private const int SIO_UDP_CONN_RESET = -1744830452;

        private readonly ObjectPool<byte[]> bufferPool;
        private readonly ObjectPool<IPEndPoint> ipEndPointPool;
        private readonly object receiveLock = new object();
        private readonly object sendLock = new object();

        private bool isClosed;
        private Socket socket;

        public UdpSocket()
        {
            bufferPool = new ObjectPool<byte[]>(() => new byte[Consts.bufferSize]);
            ipEndPointPool = new ObjectPool<IPEndPoint>(() => new IPEndPoint(IPAddress.Any, 0));
        }

        public UdpSocket(UdpSocket socket, NetEndPoint remoteEndPoint) : this()
        {
            this.socket = socket.socket;
            this.remoteEndPoint = remoteEndPoint;
        }

        public bool isBound => socket.IsBound;

        public IUdpSocketIOListener listener { get; set; }
        public bool isConnected { get; private set; }

        public NetEndPoint localEndPoint { get; private set; }
        public NetEndPoint remoteEndPoint { get; private set; }

        public void Bind(NetEndPoint endPoint)
        {
            localEndPoint = endPoint;
            isConnected = true;

            IPEndPoint ipEndPoint = ipEndPointPool.Rent();
            ipEndPoint.Address = endPoint.address;
            ipEndPoint.Port = endPoint.port;
            Bind(ipEndPoint);
            ipEndPointPool.Pay(ipEndPoint);
        }

        public void Connect(NetEndPoint endPoint)
        {
            remoteEndPoint = endPoint;
        }

        public void Close()
        {
            lock (sendLock)
            {
                lock (receiveLock)
                {
                    if (isClosed || socket == null) return;
                    socket.Close();
                    socket = null;
                    isClosed = true;
                }
            }
        }

        private void Bind(IPEndPoint endPoint)
        {
            socket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp)
            {
                ReceiveBufferSize = Consts.bufferSize,
                SendBufferSize = Consts.bufferSize,
                Blocking = false
            };
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            try
            {
                socket.DontFragment = true;
            }
            catch (Exception)
            {
                Logger.Log("Don't Fragment not supported.");
            }

            try
            {
                socket.IOControl((IOControlCode) SIO_UDP_CONN_RESET, new byte[] {0, 0, 0, 0}, null);
            }
            catch (Exception)
            {
                Logger.Log("Error setting SIO_UDP_CONN_RESET. Maybe not running on Windows.");
            }

            socket.Bind(endPoint);

            isClosed = false;

            Logger.Log($"Listening on {endPoint}");
        }

        public override string ToString()
        {
            return $"{{EndPoint-{remoteEndPoint}}}";
        }

        #region Read & Write

        public void Receive()
        {
            lock (receiveLock)
            {
                if (socket == null) return;
                if (!socket.Poll(1, SelectMode.SelectRead)) return;
                
                var buffer = bufferPool.Rent();
                var endPoint = ipEndPointPool.Rent();
                endPoint.Address = IPAddress.Any;
                endPoint.Port = 0;
                EndPoint ep = endPoint;

                var count = socket.ReceiveFrom(buffer, 0, Consts.bufferSize, SocketFlags.None, ref ep);
                if (count > 0) listener?.SocketDidReceiveBytes(this, buffer, count, From(ep));

                bufferPool.Pay(buffer);
                ipEndPointPool.Pay(endPoint);
            }
        }

        public void Send(byte[] bytes, int count, NetEndPoint to)
        {
            lock (sendLock)
            {
                if (socket == null) return;
                if (!socket.Poll(1, SelectMode.SelectWrite)) return;
                
                var endPoint = ipEndPointPool.Rent();
                From(to, ref endPoint);
                var written = socket.SendTo(bytes, 0, count, SocketFlags.None, endPoint);
                if (written > 0) listener?.SocketDidWriteBytes(this, written, to);
                ipEndPointPool.Pay(endPoint);
            }
        }

        #endregion

        #region Equatable Methods

        private static void From(NetEndPoint ep, ref IPEndPoint endPoint)
        {
            endPoint.Address = ep.address;
            endPoint.Port = ep.port;
        }

        private static NetEndPoint From(EndPoint ep)
        {
            var endPoint = (IPEndPoint) ep;
            return new NetEndPoint(endPoint.Address, endPoint.Port);
        }

        public override bool Equals(object obj)
        {
            if (obj is UdpSocket other) return remoteEndPoint.Equals(other.remoteEndPoint);
            return Equals(this, obj);
        }

        public override int GetHashCode()
        {
            return remoteEndPoint.GetHashCode();
        }

        #endregion
    }
}
