using System;
using GameNetworking.Channels;
using GameNetworking.Messages.Models;
using GameNetworking.Sockets;

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

        void Update();
    }

    public class NetworkServer : INetworkServer, ITcpServerListener<TcpSocket>, IUnreliableChannelListener {
        private readonly Object lockToken = new Object();
        private readonly TcpSocket tcpSocket;
        private readonly UdpSocket udpSocket;

        private readonly PlayerCollection<TcpSocket, ReliableChannel> socketCollection;
        private readonly PlayerCollection<NetEndPoint, INetworkServerMessageListener> identifiedCollection;

        private bool isAccepting = false;

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
            this.tcpSocket.Bind(endPoint);
            this.tcpSocket.Start();

            this.udpSocket.Bind(endPoint);

            this.listeningOnEndPoint = this.tcpSocket.localEndPoint;

            this.unreliableChannel.StartIO();
        }

        public void Stop() {
            this.tcpSocket.Stop();
            this.unreliableChannel.StopIO();
        }

        public void Close() {
            this.tcpSocket.Close();
            this.udpSocket.Close();
        }

        public void Update() {
            this.Accept();
        }

        public void Accept() {
            lock (this.lockToken) {
                if (this.isAccepting) { return; }
                this.isAccepting = true;
            }

            this.tcpSocket.Accept();
        }

        public void Register(NetEndPoint endPoint, INetworkServerMessageListener listener) {
            this.identifiedCollection.Add(endPoint, listener);
        }

        public void Unregister(NetEndPoint endPoint) {
            this.identifiedCollection.Remove(endPoint);
        }

        void ITcpServerListener<TcpSocket>.SocketDidAccept(TcpSocket socket) {
            if (socket == null) { return; }

            socket.serverListener = this;

            var reliable = new ReliableChannel(socket);

            reliable.StartIO();

            this.socketCollection.Add(socket, reliable);
            this.listener?.NetworkServerDidAcceptPlayer(reliable, this.unreliableChannel);

            lock (this.lockToken) { this.isAccepting = false; }
        }

        void ITcpServerListener<TcpSocket>.SocketDidDisconnect(TcpSocket socket) {
            var channel = this.socketCollection.Remove(socket);
            if (channel == null) { return; }
            this.listener?.NetworkServerPlayerDidDisconnect(channel);
        }

        void IUnreliableChannelListener.ChannelDidReceiveMessage(UnreliableChannel channel, MessageContainer container, NetEndPoint from) {
            if (this.identifiedCollection.TryGetPlayer(from, out INetworkServerMessageListener listener)) {
                listener?.NetworkServerDidReceiveMessage(container);
            } else {
                this.listener?.NetworkServerDidReceiveUnidentifiedMessage(container, from);
            }
        }
    }
}