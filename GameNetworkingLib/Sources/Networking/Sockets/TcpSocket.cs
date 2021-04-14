using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using GameNetworking.Commons;
using Logging;

namespace GameNetworking.Networking.Sockets
{
    public interface ITcpServerListener<in TDerived> where TDerived : ITcpSocket<TDerived>
    {
        void SocketDidAccept(TDerived socket);
        void SocketDidDisconnect(TDerived socket);
    }

    public interface ITcpClientListener
    {
        void SocketDidConnect();
        void SocketDidTimeout();
        void SocketDidDisconnect();
    }

    public interface ITcpSocketIOListener<TDerived>
        where TDerived : ITcpSocket<TDerived>
    {
        void SocketDidReceiveBytes(TDerived socket, byte[] bytes, int count);
        void SocketDidSendBytes(TDerived socket, int count);
    }

    public interface ITcpSocket<TDerived> : ISocket<TDerived>, IEquatable<TDerived>
        where TDerived : ITcpSocket<TDerived>
    {
        ITcpServerListener<TDerived> serverListener { get; set; }
        ITcpClientListener clientListener { get; set; }

        void Start();
        void Stop();

        void Send(byte[] bytes, int count);

        void Disconnect();
    }

    public sealed class TcpSocket : ITcpSocket<TcpSocket>
    {
        private readonly object acceptLock = new object();
        private readonly ObjectPool<byte[]> bufferPool;
        private readonly ObjectPool<IPEndPoint> ipEndPointPool;
        private readonly object receiveLock = new object();
        private readonly object sendLock = new object();
        private bool hasBeenConnected;

        private bool isClosed;
        private Socket socket;

        public TcpSocket() : this(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
        }

        private TcpSocket(Socket socket)
        {
            bufferPool = new ObjectPool<byte[]>(() => new byte[Consts.bufferSize]);
            ipEndPointPool = new ObjectPool<IPEndPoint>(() => new IPEndPoint(IPAddress.Any, 0));
            this.socket = socket;

            this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            this.socket.NoDelay = true;
            this.socket.Blocking = false;
            this.socket.ReceiveTimeout = 1000;
            this.socket.SendTimeout = 1000;
            this.socket.ReceiveBufferSize = Consts.bufferSize;
            this.socket.SendBufferSize = Consts.bufferSize;

            PopulateEndPoints();
        }

        public bool isBound => socket?.IsBound ?? false;

        public ITcpSocketIOListener<TcpSocket> ioListener { get; set; }

        public bool isConnected => socket?.Connected ?? false;

        public NetEndPoint localEndPoint { get; private set; }
        public NetEndPoint remoteEndPoint { get; private set; }
        public ITcpServerListener<TcpSocket> serverListener { get; set; }
        public ITcpClientListener clientListener { get; set; }

        public void Close()
        {
            CheckClosed();
        }

        #region IEquatable

        bool IEquatable<TcpSocket>.Equals(TcpSocket other)
        {
            return remoteEndPoint.Equals(other.remoteEndPoint);
        }

        #endregion

        private void PopulateEndPoints()
        {
            if (socket.LocalEndPoint != null)
            {
                var ipep = (IPEndPoint) socket.LocalEndPoint;
                localEndPoint = new NetEndPoint(ipep.Address, ipep.Port);
            }

            if (socket.RemoteEndPoint != null)
            {
                var ipep = (IPEndPoint) socket.RemoteEndPoint;
                remoteEndPoint = new NetEndPoint(ipep.Address, ipep.Port);
            }
        }

        private void CheckClosed()
        {
            lock (acceptLock)
            {
                lock (receiveLock)
                {
                    lock (sendLock)
                    {
                        if (isClosed && socket == null) return;

                        try
                        {
                            if (isConnected) socket.Shutdown(SocketShutdown.Both);
                        }
                        finally
                        {
                            socket.Close();
                        }

                        socket = null;
                        isClosed = true;
                        hasBeenConnected = false;
                    }
                }
            }
        }

        #region Server

        public void Start()
        {
            socket.Listen(0);

            ThreadPool.QueueUserWorkItem(_ =>
            {
                Thread.CurrentThread.Name = "TcpSocket Accept Thread";
                ThreadChecker.ConfigureAccept(Thread.CurrentThread);
                do
                {
                    Accept();
                } while (socket != null);

                Logger.Log("TcpSocket Accept Thread EXITING!");
                ThreadChecker.ConfigureAccept(null);
            });
        }

        public void Stop()
        {
            CheckClosed();
        }

        public void Bind(NetEndPoint endPoint)
        {
            localEndPoint = endPoint;

            var ipep = ipEndPointPool.Rent();
            From(endPoint, ref ipep);
            socket.Bind(ipep);
            ipEndPointPool.Pay(ipep);
        }

        private void Accept()
        {
            lock (acceptLock)
            {
                if (socket == null) return;

                if (!socket.Poll(1, SelectMode.SelectRead)) return;

                var accepted = socket.Accept();
                if (accepted != null) serverListener?.SocketDidAccept(new TcpSocket(accepted) {hasBeenConnected = true});
            }
        }

        #endregion

        #region Client

        private struct ConnectState
        {
            public IPEndPoint ipEndPoint;
        }

        public void Connect(NetEndPoint endPoint)
        {
            remoteEndPoint = endPoint;

            var ipep = ipEndPointPool.Rent();
            From(endPoint, ref ipep);
            socket.BeginConnect(ipep, ConnectResult, new ConnectState {ipEndPoint = ipep});
        }

        private void ConnectResult(IAsyncResult ar)
        {
            var state = (ConnectState) ar.AsyncState;
            if (socket.Connected)
            {
                socket.EndConnect(ar);
                PopulateEndPoints();
                clientListener?.SocketDidConnect();
                hasBeenConnected = true;
            }
            else
            {
                clientListener?.SocketDidTimeout();
                CheckClosed();
            }

            ipEndPointPool.Pay(state.ipEndPoint);
        }

        public void Disconnect()
        {
            socket?.Disconnect(false);
            Close();
            clientListener?.SocketDidDisconnect();
        }

        #endregion

        #region Read & Write

        private void NotifyClosing()
        {
            serverListener?.SocketDidDisconnect(this);
            clientListener?.SocketDidDisconnect();
            CheckClosed();
        }

        public void Receive()
        {
            lock (receiveLock)
            {
                if (socket == null) return;

                if (socket.Poll(1, SelectMode.SelectRead))
                {
                    if (CheckDisconnected())
                    {
                        NotifyClosing();
                    }
                    else
                    {
                        var buffer = bufferPool.Rent();
                        var count = socket.Receive(buffer, 0, Consts.bufferSize, SocketFlags.None, out var error);
                        if (count > 0) ioListener?.SocketDidReceiveBytes(this, buffer, count);

                        bufferPool.Pay(buffer);

                        if (CheckSocketError(error)) NotifyClosing();
                    }
                }
            }
        }

        public void Send(byte[] bytes, int count)
        {
            lock (sendLock)
            {
                if (socket == null) return;

                if (socket.Poll(1, SelectMode.SelectWrite))
                {
                    var written = socket.Send(bytes, 0, count, SocketFlags.None, out var error);
                    if (written > 0) ioListener?.SocketDidSendBytes(this, written);

                    if (CheckSocketError(error)) NotifyClosing();
                }
            }
        }

        #endregion

        #region Private Methods

        private bool CheckDisconnected()
        {
            return hasBeenConnected && !isConnected;
        }

        private bool CheckSocketError(SocketError error)
        {
            switch (error)
            {
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

        private void From(NetEndPoint ep, ref IPEndPoint endPoint)
        {
            endPoint.Address = ep.address;
            endPoint.Port = ep.port;
        }

        #endregion
    }
}