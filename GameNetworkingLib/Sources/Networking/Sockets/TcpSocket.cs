using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using GameNetworking.Commons;
using Logging;

namespace GameNetworking.Networking.Sockets
{
    public interface ITcpServerListener
    {
        void SocketDidAccept(ITcpSocket socket);
        void SocketDidDisconnect(ITcpSocket socket);
    }

    public interface ITcpClientListener
    {
        void SocketDidConnect();
        void SocketDidTimeout();
        void SocketDidDisconnect();
    }

    public interface ITcpSocketIOListener
    {
        void SocketDidReceiveBytes(ITcpSocket socket, byte[] bytes, int count);
        void SocketDidSendBytes(ITcpSocket socket, int count);
    }

    public interface ITcpSocket : ISocket, IEquatable<ITcpSocket>
    {
        ITcpServerListener serverListener { get; set; }
        ITcpClientListener clientListener { get; set; }
        ITcpSocketIOListener ioListener { get; set; }

        void Start();
        void Stop();

        void Send(byte[] bytes, int count);
        void Receive();

        void Disconnect();
    }

    public sealed class TcpSocket : ITcpSocket, IEquatable<TcpSocket>
    {
        private readonly ObjectPool<byte[]> _bufferPool;
        private readonly ObjectPool<IPEndPoint> ipEndPointPool;
        private readonly object _acceptLock = new object();
        private readonly object _receiveLock = new object();
        private readonly object _sendLock = new object();

        private bool _hasBeenConnected;
        private bool _isClosed;
        private Socket _socket;

        public TcpSocket() : this(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
            //
        }

        private TcpSocket(Socket socket)
        {
            _bufferPool = new ObjectPool<byte[]>(() => new byte[Consts.bufferSize]);
            ipEndPointPool = new ObjectPool<IPEndPoint>(() => new IPEndPoint(IPAddress.Any, 0));
            _socket = socket;

            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _socket.NoDelay = true;
            _socket.Blocking = false;
            _socket.ReceiveTimeout = 1000;
            _socket.SendTimeout = 1000;
            _socket.ReceiveBufferSize = Consts.bufferSize;
            _socket.SendBufferSize = Consts.bufferSize;

            PopulateEndPoints();
        }

        public bool isBound => _socket?.IsBound ?? false;

        public ITcpSocketIOListener ioListener { get; set; }

        public bool isConnected => _socket?.Connected ?? false;

        public NetEndPoint localEndPoint { get; private set; }
        public NetEndPoint remoteEndPoint { get; private set; }
        public ITcpServerListener serverListener { get; set; }
        public ITcpClientListener clientListener { get; set; }

        public void Close()
        {
            CheckClosed();
        }

        #region IEquatable

        bool IEquatable<ITcpSocket>.Equals(ITcpSocket other)
        {
            return remoteEndPoint.Equals(other?.remoteEndPoint);
        }

        bool IEquatable<TcpSocket>.Equals(TcpSocket other)
        {
            return remoteEndPoint.Equals(other?.remoteEndPoint);
        }

        #endregion

        private void PopulateEndPoints()
        {
            IPEndPoint ipEndPoint;
            if (_socket.LocalEndPoint != null)
            {
                ipEndPoint = (IPEndPoint) _socket.LocalEndPoint;
                localEndPoint = new NetEndPoint(ipEndPoint.Address, ipEndPoint.Port);
            }

            if (_socket.RemoteEndPoint == null) return;
            
            ipEndPoint = (IPEndPoint) _socket.RemoteEndPoint;
            remoteEndPoint = new NetEndPoint(ipEndPoint.Address, ipEndPoint.Port);
        }

        private void CheckClosed()
        {
            lock (_acceptLock)
            {
                lock (_receiveLock)
                {
                    lock (_sendLock)
                    {
                        if (_isClosed && _socket == null) return;

                        try
                        {
                            if (isConnected) _socket.Shutdown(SocketShutdown.Both);
                        }
                        finally
                        {
                            _socket.Close();
                        }

                        _socket = null;
                        _isClosed = true;
                        _hasBeenConnected = false;
                    }
                }
            }
        }

        #region Server

        public void Start()
        {
            _socket.Listen(0);

            ThreadPool.QueueUserWorkItem(_ =>
            {
                Thread.CurrentThread.Name = "TcpSocket Accept Thread";
                ThreadChecker.ConfigureAccept(Thread.CurrentThread);
                do
                {
                    Accept();
                } while (_socket != null);

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

            IPEndPoint ipEndPoint = ipEndPointPool.Rent();
            From(endPoint, ref ipEndPoint);
            _socket.Bind(ipEndPoint);
            ipEndPointPool.Pay(ipEndPoint);
        }

        private void Accept()
        {
            lock (_acceptLock)
            {
                if (_socket == null) return;

                if (!_socket.Poll(1, SelectMode.SelectRead)) return;

                Socket accepted = _socket.Accept();
                serverListener?.SocketDidAccept(new TcpSocket(accepted)
                {
                    _hasBeenConnected = true
                });
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

            IPEndPoint ipEndPoint = ipEndPointPool.Rent();
            From(endPoint, ref ipEndPoint);
            _socket.BeginConnect(ipEndPoint, ConnectResult, new ConnectState {ipEndPoint = ipEndPoint});
        }

        private void ConnectResult(IAsyncResult ar)
        {
            var state = (ConnectState) ar.AsyncState;
            if (_socket.Connected)
            {
                _socket.EndConnect(ar);
                PopulateEndPoints();
                clientListener?.SocketDidConnect();
                _hasBeenConnected = true;
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
            _socket?.Disconnect(false);
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
            lock (_receiveLock)
            {
                if (_socket == null) return;

                if (!_socket.Poll(1, SelectMode.SelectRead)) return;
                
                if (CheckDisconnected())
                {
                    NotifyClosing();
                    return;
                }

                var buffer = _bufferPool.Rent();
                var count = _socket.Receive(buffer, 0, Consts.bufferSize, SocketFlags.None, out var error);
                if (count > 0) ioListener?.SocketDidReceiveBytes(this, buffer, count);

                _bufferPool.Pay(buffer);

                if (CheckSocketError(error)) NotifyClosing();
            }
        }

        public void Send(byte[] bytes, int count)
        {
            lock (_sendLock)
            {
                if (_socket == null) return;

                if (!_socket.Poll(1, SelectMode.SelectWrite)) return;
                
                var written = _socket.Send(bytes, 0, count, SocketFlags.None, out var error);
                if (written > 0) ioListener?.SocketDidSendBytes(this, written);

                if (CheckSocketError(error)) NotifyClosing();
            }
        }

        #endregion

        #region Private Methods

        private bool CheckDisconnected()
        {
            return _hasBeenConnected && !isConnected;
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
