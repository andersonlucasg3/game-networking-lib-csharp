using GameNetworking.Channels;
using GameNetworking.Commons;
using GameNetworking.Messages.Models;
using GameNetworking.Networking.Sockets;

namespace GameNetworking.Networking
{
    public interface INetworkServerListener
    {
        void NetworkServerReliableChannelConnected(ReliableChannel reliable);
        void NetworkServerUnreliableChannelConnected(UnreliableChannel unreliable);
        void NetworkServerReliableChannelDisconnected(ReliableChannel channel);
    }

    public interface INetworkServerUnidentifiedMessageListener
    {
        void NetworkServerDidReceiveUnidentifiedMessage(MessageContainer container, NetEndPoint from);   
    }

    public interface INetworkServerMessageListener
    {
        void NetworkServerDidReceiveMessage(MessageContainer container);
    }

    public class NetworkServer : ITcpServerListener, IChannelListener<UnreliableChannel>
    {
        private readonly PlayerCollection<NetEndPoint, INetworkServerMessageListener> _identifiedCollection;

        private readonly PlayerCollection<ITcpSocket, ReliableChannel> _socketCollection;
        private readonly TcpSocket _tcpSocket;
        private readonly UdpSocket _udpSocket;

        public UnreliableChannel unreliableChannel { get; }
        public NetEndPoint listeningOnEndPoint { get; private set; }

        public INetworkServerListener listener { get; set; }
        public INetworkServerUnidentifiedMessageListener unidentifiedMessageListener { get; set; }
        
        public NetworkServer(TcpSocket tcpSocket, UdpSocket udpSocket)
        {
            _tcpSocket = tcpSocket;
            _udpSocket = udpSocket;

            _tcpSocket.serverListener = this;

            _socketCollection = new PlayerCollection<ITcpSocket, ReliableChannel>();
            _identifiedCollection = new PlayerCollection<NetEndPoint, INetworkServerMessageListener>();

            if (_udpSocket == null) return;
            
            unreliableChannel = new UnreliableChannel(_udpSocket) {listener = this};
        }

        void ITcpServerListener.SocketDidAccept(ITcpSocket socket)
        {
            ThreadChecker.AssertAcceptThread();

            if (socket == null) return;

            socket.serverListener = this;

            ReliableChannel reliable = new ReliableChannel(socket);
            ReliableChannel.Add(reliable);

            _socketCollection.Add(socket, reliable);
            listener?.NetworkServerReliableChannelConnected(reliable);
        }

        void ITcpServerListener.SocketDidDisconnect(ITcpSocket socket)
        {
            ThreadChecker.AssertReliableChannel();

            var channel = _socketCollection.Remove(socket);
            if (channel == null) return;

            ReliableChannel.Remove(channel);
            listener?.NetworkServerReliableChannelDisconnected(channel);
        }

        void IChannelListener<UnreliableChannel>.ChannelDidReceiveMessage(UnreliableChannel channel, MessageContainer container)
        {
            ThreadChecker.AssertUnreliableChannel();

            if (_identifiedCollection.TryGetPlayer(container.remoteEndPoint, out var messageListener)) messageListener?.NetworkServerDidReceiveMessage(container);
            else unidentifiedMessageListener?.NetworkServerDidReceiveUnidentifiedMessage(container, container.remoteEndPoint);
        }

        public void StartReliable(NetEndPoint endPoint)
        {
            ThreadChecker.AssertMainThread();

            _tcpSocket.Bind(endPoint);
            _tcpSocket.Start();

            listeningOnEndPoint = _tcpSocket.localEndPoint;

            ReliableChannel.StartIO();
        }

        public void StartUnreliable(NetEndPoint endPoint)
        {
            ThreadChecker.AssertMainThread();
            
            _udpSocket.Bind(endPoint);
            
            unreliableChannel?.StartIO();
        }

        public void StopReliable()
        {
            ThreadChecker.AssertMainThread();

            ReliableChannel.StopIO();
            _tcpSocket.Stop();
        }

        public void StopUnreliable()
        {
            unreliableChannel?.StopIO();
        }

        public void StopAll()
        {
            StopReliable();
            StopUnreliable();
        }

        public void Close()
        {
            ThreadChecker.AssertMainThread();

            _tcpSocket.Close();
            _udpSocket.Close();
        }

        public void Register(NetEndPoint endPoint, INetworkServerMessageListener messageListener)
        {
            _identifiedCollection.Add(endPoint, messageListener);
        }

        public void Unregister(NetEndPoint endPoint)
        {
            _identifiedCollection.Remove(endPoint);
        }
    }
}
