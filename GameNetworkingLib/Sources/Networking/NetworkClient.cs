using System.Threading;
using GameNetworking.Channels;
using GameNetworking.Messages.Models;
using GameNetworking.Sockets;

namespace GameNetworking.Networking {
    public interface IMessageSender {
        void Send(ITypedMessage message, Channel channel);
    }

    public interface INetworkClientListener {
        void NetworkClientDidConnect();
        void NetworkClientConnectDidTimeout();
        void NetworkClientDidDisconnect();

        void NetworkClientDidReceiveMessage(MessageContainer container);
        void NetworkClientDidReceiveMessage(MessageContainer container, NetEndPoint from);
    }

    public interface INetworkClient: IMessageSender {
        INetworkClientListener listener { get; set; }

        NetEndPoint localEndPoint { get; }
        NetEndPoint remoteEndPoint { get; }

        void Connect(string host, int port);
        void Disconnect();
    }

    public class NetworkClient : INetworkClient, ITcpClientListener, IChannelListener {
        private readonly TcpSocket tcpSocket;
        private readonly UdpSocket udpSocket;

        private readonly ReliableChannel reliableChannel;
        private readonly UnreliableChannel unreliableChannel;

        public NetEndPoint localEndPoint => this.tcpSocket.localEndPoint;
        public NetEndPoint remoteEndPoint => this.tcpSocket.remoteEndPoint;

        public INetworkClientListener listener { get; set; }

        public NetworkClient(TcpSocket tcpSocket, UdpSocket udpSocket) {
            this.tcpSocket = tcpSocket;
            this.udpSocket = udpSocket;

            this.tcpSocket.clientListener = this;

            this.reliableChannel = new ReliableChannel(this.tcpSocket);
            this.unreliableChannel = new UnreliableChannel(this.udpSocket);

            this.reliableChannel.listener = this;
            this.unreliableChannel.listener = this;
        }

        public void Connect(string host, int port) => this.tcpSocket.Connect(new NetEndPoint(host, port));
        internal void ReconnectUnreliable(NetEndPoint remote) => this.udpSocket.Connect(remote);

        public void Disconnect() {
            this.reliableChannel.CloseChannel();
        }

        public void Send(ITypedMessage message, Channel channel) {
            this.GetChannel(channel).Send(message);
        }

        private IChannel GetChannel(Channel channel) {
            switch (channel) {
            case Channel.reliable: return this.reliableChannel;
            case Channel.unreliable: return this.unreliableChannel;
            default: return null;
            }
        }

        void ITcpClientListener.SocketDidConnect() {
            this.udpSocket.Bind(this.tcpSocket.localEndPoint);
            this.udpSocket.Connect(this.tcpSocket.remoteEndPoint);

            this.listener?.NetworkClientDidConnect();
        }

        void ITcpClientListener.SocketDidTimeout() {
            this.listener?.NetworkClientConnectDidTimeout();

            this.tcpSocket.Close();
        }

        void ITcpClientListener.SocketDidDisconnect() {
            this.listener?.NetworkClientDidDisconnect();

            this.tcpSocket.Close();
        }

        void IChannelListener.ChannelDidReceiveMessage(MessageContainer container, NetEndPoint from) {
            this.listener?.NetworkClientDidReceiveMessage(container, from);
        }
    }
}