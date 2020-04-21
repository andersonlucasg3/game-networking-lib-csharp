using GameNetworking.Channels;
using GameNetworking.Messages.Models;
using GameNetworking.Sockets;

namespace GameNetworking.Networking {
    public interface INetworkClientListener {
        void NetworkClientDidConnect();
        void NetworkClientConnectDidTimeout();
        void NetworkClientDidDisconnect();

        void NetworkClientDidReceiveMessage(MessageContainer container);
    }

    public interface INetworkClient {
        INetworkClientListener listener { get; set; }

        void Connect(string host, int port);
        void Disconnect();

        void Receive();
        void Send(ITypedMessage message, Channel channel);
        void Flush();
    }

    public class NetworkClient : INetworkClient, ITcpClientListener, IChannelListener {
        private readonly ITcpSocket tcpSocket;
        private readonly ISocket udpSocket;

        private readonly ReliableChannel reliableChannel;
        private readonly UnreliableChannel unreliableChannel;

        public INetworkClientListener listener { get; set; }

        public NetworkClient(ITcpSocket tcpSocket, ISocket udpSocket) {
            this.tcpSocket = tcpSocket;
            this.udpSocket = udpSocket;

            this.reliableChannel = new ReliableChannel(this.tcpSocket);
            this.unreliableChannel = new UnreliableChannel(this.udpSocket);

            this.reliableChannel.listener = this;
            this.unreliableChannel.listener = this;
        }

        public void Connect(string host, int port) {
            this.tcpSocket.Connect(new NetEndPoint(host, port));
        }

        public void Disconnect() {
            this.reliableChannel.CloseChannel();
        }

        public void Receive() {
            this.reliableChannel.Receive();
            this.unreliableChannel.Receive();
        }

        public void Send(ITypedMessage message, Channel channel) {
            this.GetChannel(channel).Send(message);
        }

        public void Flush() {
            this.reliableChannel.Flush();
            this.unreliableChannel.Flush();
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

        void IChannelListener.ChannelDidReceiveMessage(MessageContainer container) {
            this.listener?.NetworkClientDidReceiveMessage(container);
        }
    }
}