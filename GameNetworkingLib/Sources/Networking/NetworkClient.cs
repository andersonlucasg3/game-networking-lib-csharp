using System.Threading;
using GameNetworking.Channels;
using GameNetworking.Messages.Models;
using GameNetworking.Sockets;

namespace GameNetworking.Networking {
    public interface INetworkClientListener {
        void NetworkClientDidConnect();
        void NetworkClientConnectDidTimeout();
        void NetworkClientDidDisconnect();

        void NetworkClientDidReceiveMessage(MessageContainer container);
        void NetworkClientDidReceiveMessage(MessageContainer container, NetEndPoint from);
    }

    public interface INetworkClient {
        NetEndPoint localEndPoint { get; }
        NetEndPoint remoteEndPoint { get; }

        ReliableChannel reliableChannel { get; }
        UnreliableChannel unreliableChannel { get; }

        INetworkClientListener listener { get; set; }

        void Connect(string host, int port);
        void Disconnect();
    }

    public class NetworkClient : INetworkClient, ITcpClientListener, IReliableChannelListener, IUnreliableChannelListener {
        private readonly TcpSocket tcpSocket;
        private readonly UdpSocket udpSocket;

        public NetEndPoint localEndPoint => this.tcpSocket.localEndPoint;
        public NetEndPoint remoteEndPoint => this.tcpSocket.remoteEndPoint;

        public ReliableChannel reliableChannel { get; private set; }
        public UnreliableChannel unreliableChannel { get; private set; }

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
            this.unreliableChannel.StopIO();
        }

        void ITcpClientListener.SocketDidConnect() {
            this.udpSocket.Bind(this.tcpSocket.localEndPoint);
            this.udpSocket.Connect(this.tcpSocket.remoteEndPoint);

            this.listener?.NetworkClientDidConnect();

            this.reliableChannel.StartIO();
            this.unreliableChannel.StartIO();
        }

        void ITcpClientListener.SocketDidTimeout() {
            this.listener?.NetworkClientConnectDidTimeout();

            this.tcpSocket.Close();
        }

        void ITcpClientListener.SocketDidDisconnect() {
            this.listener?.NetworkClientDidDisconnect();

            this.tcpSocket.Close();
        }

        void IReliableChannelListener.ChannelDidReceiveMessage(ReliableChannel channel, MessageContainer container) {
            this.listener?.NetworkClientDidReceiveMessage(container);
        }

        void IUnreliableChannelListener.ChannelDidReceiveMessage(UnreliableChannel channel, MessageContainer container, NetEndPoint from) {
            this.listener?.NetworkClientDidReceiveMessage(container, from);
        }
    }
}