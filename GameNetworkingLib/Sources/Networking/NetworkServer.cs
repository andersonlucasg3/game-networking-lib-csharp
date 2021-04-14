using GameNetworking.Channels;
using GameNetworking.Commons;
using GameNetworking.Messages.Models;
using GameNetworking.Networking.Sockets;

namespace GameNetworking.Networking
{
    public interface INetworkServerListener
    {
        void NetworkServerDidAcceptPlayer(ReliableChannel reliable, UnreliableChannel unreliable);
        void NetworkServerPlayerDidDisconnect(ReliableChannel channel);
        void NetworkServerDidReceiveUnidentifiedMessage(MessageContainer container, NetEndPoint from);
    }

    public interface INetworkServerMessageListener
    {
        void NetworkServerDidReceiveMessage(MessageContainer container);
    }

    public class NetworkServer : ITcpServerListener<TcpSocket>, IUnreliableChannelListener
    {
        private readonly PlayerCollection<NetEndPoint, INetworkServerMessageListener> _identifiedCollection;

        private readonly PlayerCollection<TcpSocket, ReliableChannel> _socketCollection;
        private readonly TcpSocket _tcpSocket;
        private readonly UdpSocket _udpSocket;

        public NetworkServer(TcpSocket tcpSocket, UdpSocket udpSocket)
        {
            _tcpSocket = tcpSocket;
            _udpSocket = udpSocket;

            _tcpSocket.serverListener = this;

            _socketCollection = new PlayerCollection<TcpSocket, ReliableChannel>();
            _identifiedCollection = new PlayerCollection<NetEndPoint, INetworkServerMessageListener>();

            unreliableChannel = new UnreliableChannel(_udpSocket) {listener = this};
        }

        public UnreliableChannel unreliableChannel { get; }
        public NetEndPoint listeningOnEndPoint { get; private set; }

        public INetworkServerListener listener { get; set; }

        void ITcpServerListener<TcpSocket>.SocketDidAccept(TcpSocket socket)
        {
            ThreadChecker.AssertAcceptThread();

            if (socket == null) return;

            socket.serverListener = this;

            var reliable = new ReliableChannel(socket);
            ReliableChannel.Add(reliable);

            _socketCollection.Add(socket, reliable);
            listener?.NetworkServerDidAcceptPlayer(reliable, unreliableChannel);
        }

        void ITcpServerListener<TcpSocket>.SocketDidDisconnect(TcpSocket socket)
        {
            ThreadChecker.AssertReliableChannel();

            var channel = _socketCollection.Remove(socket);
            if (channel == null) return;

            ReliableChannel.Remove(channel);
            listener?.NetworkServerPlayerDidDisconnect(channel);
        }

        void IUnreliableChannelListener.ChannelDidReceiveMessage(UnreliableChannel channel, MessageContainer container, NetEndPoint from)
        {
            ThreadChecker.AssertUnreliableChannel();

            if (_identifiedCollection.TryGetPlayer(from, out var messageListener))
                messageListener?.NetworkServerDidReceiveMessage(container);
            else
                listener?.NetworkServerDidReceiveUnidentifiedMessage(container, @from);
        }

        public void Start(NetEndPoint endPoint)
        {
            ThreadChecker.AssertMainThread();

            _tcpSocket.Bind(endPoint);
            _tcpSocket.Start();

            _udpSocket.Bind(endPoint);

            listeningOnEndPoint = _tcpSocket.localEndPoint;

            ReliableChannel.StartIO();
            unreliableChannel.StartIO();
        }

        public void Stop()
        {
            ThreadChecker.AssertMainThread();

            ReliableChannel.StopIO();
            _tcpSocket.Stop();
            unreliableChannel.StopIO();
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