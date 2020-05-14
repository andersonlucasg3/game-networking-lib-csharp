using GameNetworking.Channels;
using GameNetworking.Commons;
using GameNetworking.Messages.Models;
using GameNetworking.Networking.Sockets;

namespace GameNetworking.Networking {
    public interface INetworkServerListener {
        void NetworkServerDidAcceptPlayer(ReliableChannel reliable, UnreliableChannel unreliable);
        void NetworkServerPlayerDidDisconnect(ReliableChannel channel);
        void NetworkServerDidReceiveUnidentifiedMessage(MessageContainer container, NetEndPoint from);
    }

    public interface INetworkServerMessageListener {
        void NetworkServerDidReceiveMessage(MessageContainer container);
    }

    public interface INetworkServer {
        UnreliableChannel unreliableChannel { get; }

        INetworkServerListener listener { get; set; }

        NetEndPoint listeningOnEndPoint { get; }

        void Start(NetEndPoint endPoint);
        void Stop();
    }

    public class NetworkServer : INetworkServer, ITcpServerListener<TcpSocket>, IUnreliableChannelListener {
        private readonly TcpSocket tcpSocket;
        private readonly UdpSocket udpSocket;

        private readonly PlayerCollection<TcpSocket, ReliableChannel> socketCollection;
        private readonly PlayerCollection<NetEndPoint, INetworkServerMessageListener> identifiedCollection;

        public UnreliableChannel unreliableChannel { get; }
        public NetEndPoint listeningOnEndPoint { get; private set; }

        public INetworkServerListener listener { get; set; }

        public NetworkServer(TcpSocket tcpSocket, UdpSocket udpSocket) {
            this.tcpSocket = tcpSocket;
            this.udpSocket = udpSocket;

            this.tcpSocket.serverListener = this;

            this.socketCollection = new PlayerCollection<TcpSocket, ReliableChannel>();
            this.identifiedCollection = new PlayerCollection<NetEndPoint, INetworkServerMessageListener>();

            this.unreliableChannel = new UnreliableChannel(this.udpSocket) { listener = this };
        }

        public void Start(NetEndPoint endPoint) {
            ThreadChecker.AssertMainThread();

            this.tcpSocket.Bind(endPoint);
            this.tcpSocket.Start();

            this.udpSocket.Bind(endPoint);

            this.listeningOnEndPoint = this.tcpSocket.localEndPoint;

            ReliableChannel.StartIO();
            this.unreliableChannel.StartIO();
        }

        public void Stop() {
            ThreadChecker.AssertMainThread();

            ReliableChannel.StopIO();
            this.tcpSocket.Stop();
            this.unreliableChannel.StopIO();
        }

        public void Close() {
            ThreadChecker.AssertMainThread();

            this.tcpSocket.Close();
            this.udpSocket.Close();
        }

        public void Register(NetEndPoint endPoint, INetworkServerMessageListener listener) {
            this.identifiedCollection.Add(endPoint, listener);
        }

        public void Unregister(NetEndPoint endPoint) {
            this.identifiedCollection.Remove(endPoint);
        }

        void ITcpServerListener<TcpSocket>.SocketDidAccept(TcpSocket socket) {
            ThreadChecker.AssertAcceptThread();

            if (socket == null) { return; }

            socket.serverListener = this;

            var reliable = new ReliableChannel(socket);
            ReliableChannel.Add(reliable);

            this.socketCollection.Add(socket, reliable);
            this.listener?.NetworkServerDidAcceptPlayer(reliable, this.unreliableChannel);
        }

        void ITcpServerListener<TcpSocket>.SocketDidDisconnect(TcpSocket socket) {
            ThreadChecker.AssertReliableChannel();

            var channel = this.socketCollection.Remove(socket);
            if (channel == null) { return; }
            ReliableChannel.Remove(channel);
            this.listener?.NetworkServerPlayerDidDisconnect(channel);
        }

        void IUnreliableChannelListener.ChannelDidReceiveMessage(UnreliableChannel channel, MessageContainer container, NetEndPoint from) {
            ThreadChecker.AssertUnreliableChannel();

            if (this.identifiedCollection.TryGetPlayer(from, out INetworkServerMessageListener listener)) {
                listener?.NetworkServerDidReceiveMessage(container);
            } else {
                this.listener?.NetworkServerDidReceiveUnidentifiedMessage(container, from);
            }
        }
    }
}