using System.Net;
using GameNetworking.Channels;
using GameNetworking.Messages.Models;
using GameNetworking.Networking.Sockets;

namespace GameNetworking.Networking
{
    public interface INetworkClientListener
    {
        void NetworkClientReliableChannelConnected();
        void NetworkClientReliableChannelTimedOut();
        void NetworkClientReliableChannelDisconnected();

        void NetworkClientDidReceiveMessage(MessageContainer container);
        void NetworkClientDidReceiveMessage(MessageContainer container, NetEndPoint from);
    }

    public class NetworkClient : ITcpClientListener, IChannelListener<ReliableChannel>, IChannelListener<UnreliableChannel>
    {
        private readonly TcpSocket _tcpSocket;
        private readonly UdpSocket _udpSocket;

        public NetEndPoint localEndPoint => _tcpSocket.localEndPoint;
        public NetEndPoint remoteEndPoint => _tcpSocket.remoteEndPoint;

        public ReliableChannel reliableChannel { get; }
        public UnreliableChannel unreliableChannel { get; }

        public INetworkClientListener listener { get; set; }
        
        public NetworkClient(TcpSocket tcpSocket, UdpSocket udpSocket)
        {
            _tcpSocket = tcpSocket;
            _udpSocket = udpSocket;

            _tcpSocket.clientListener = this;

            reliableChannel = new ReliableChannel(_tcpSocket);
            unreliableChannel = new UnreliableChannel(_udpSocket);

            reliableChannel.listener = this;
            unreliableChannel.listener = this;
        }

        void ITcpClientListener.SocketDidConnect()
        {
            ReliableChannel.Add(reliableChannel);

            listener?.NetworkClientReliableChannelConnected();
        }

        void ITcpClientListener.SocketDidTimeout()
        {
            listener?.NetworkClientReliableChannelTimedOut();

            _tcpSocket.Close();
        }

        void ITcpClientListener.SocketDidDisconnect()
        {
            ReliableChannel.Remove(reliableChannel);

            listener?.NetworkClientReliableChannelDisconnected();

            _tcpSocket.Close();
        }

        void IChannelListener<ReliableChannel>.ChannelDidReceiveMessage(ReliableChannel channel, MessageContainer container)
        {
            listener?.NetworkClientDidReceiveMessage(container);
        }
        
        void IChannelListener<UnreliableChannel>.ChannelDidReceiveMessage(UnreliableChannel channel, MessageContainer container)
        {
            listener?.NetworkClientDidReceiveMessage(container, container.remoteEndPoint);
        }

        public void ConnectReliable(string host, int port)
        {
            _tcpSocket.Connect(new NetEndPoint(IPAddress.Parse(host), port));
            ReliableChannel.StartIO();
        }

        public void ConnectUnreliable(string host, int port)
        {
            _udpSocket.Bind(new NetEndPoint(IPAddress.Any, port));
            _udpSocket.Connect(new NetEndPoint(IPAddress.Parse(host), port));
            
            unreliableChannel.StartIO();
        }

        public void DisconnectReliable()
        {
            reliableChannel.CloseChannel();
            ReliableChannel.StopIO();
        }

        public void DisconnectUnreliable()
        {
            unreliableChannel.CloseChannel(remoteEndPoint);
            unreliableChannel.StopIO();
        }
    }
}
