using System.Net;
using GameNetworking.Channels;
using GameNetworking.Messages.Models;
using GameNetworking.Networking.Sockets;

namespace GameNetworking.Networking
{
    public interface INetworkClientListener
    {
        void NetworkClientDidConnect();
        void NetworkClientConnectDidTimeout();
        void NetworkClientDidDisconnect();

        void NetworkClientDidReceiveMessage(MessageContainer container);
        void NetworkClientDidReceiveMessage(MessageContainer container, NetEndPoint from);
    }

    public class NetworkClient : ITcpClientListener, IChannelListener<ReliableChannel>, IChannelListener<UnreliableChannel>
    {
        private readonly TcpSocket tcpSocket;
        private readonly UdpSocket udpSocket;

        public NetworkClient(TcpSocket tcpSocket, UdpSocket udpSocket)
        {
            this.tcpSocket = tcpSocket;
            this.udpSocket = udpSocket;

            this.tcpSocket.clientListener = this;

            reliableChannel = new ReliableChannel(this.tcpSocket);
            unreliableChannel = new UnreliableChannel(this.udpSocket);

            reliableChannel.listener = this;
            unreliableChannel.listener = this;
        }

        public NetEndPoint localEndPoint => tcpSocket.localEndPoint;
        public NetEndPoint remoteEndPoint => tcpSocket.remoteEndPoint;

        public ReliableChannel reliableChannel { get; }
        public UnreliableChannel unreliableChannel { get; }

        public INetworkClientListener listener { get; set; }

        void ITcpClientListener.SocketDidConnect()
        {
            ReliableChannel.Add(reliableChannel);

            udpSocket.Bind(tcpSocket.localEndPoint);
            udpSocket.Connect(tcpSocket.remoteEndPoint);

            listener?.NetworkClientDidConnect();
        }

        void ITcpClientListener.SocketDidTimeout()
        {
            listener?.NetworkClientConnectDidTimeout();

            tcpSocket.Close();
        }

        void ITcpClientListener.SocketDidDisconnect()
        {
            ReliableChannel.Remove(reliableChannel);

            listener?.NetworkClientDidDisconnect();

            tcpSocket.Close();
        }

        void IChannelListener<ReliableChannel>.ChannelDidReceiveMessage(ReliableChannel channel, MessageContainer container)
        {
            listener?.NetworkClientDidReceiveMessage(container);
        }
        
        void IChannelListener<UnreliableChannel>.ChannelDidReceiveMessage(UnreliableChannel channel, MessageContainer container)
        {
            listener?.NetworkClientDidReceiveMessage(container, container.remoteEndPoint);
        }

        public void Connect(string host, int port)
        {
            tcpSocket.Connect(new NetEndPoint(IPAddress.Parse(host), port));
            ReliableChannel.StartIO();
            unreliableChannel.StartIO();
        }

        public void Disconnect()
        {
            reliableChannel.CloseChannel();
            unreliableChannel.CloseChannel(remoteEndPoint);
            ReliableChannel.StopIO();
            unreliableChannel.StopIO();
        }
    }
}
