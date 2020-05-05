using System;
using System.Collections.Concurrent;
using GameNetworking.Channels;
using GameNetworking.Messages.Models;
using GameNetworking.Sockets;

namespace GameNetworking.Networking {
    public interface INetworkServerListener {
        void NetworkServerDidAcceptPlayer(ReliableChannel reliable, UnreliableChannel unreliable);
        void NetworkServerPlayerDidDisconnect(ReliableChannel channel);
        void NetworkServerDidReceiveUnidentifiedMessage(MessageContainer container, NetEndPoint from);
    }

    public interface INetworkServer {
        ReliableChannel reliableChannel { get; }
        UnreliableChannel unreliableChannel { get; }

        INetworkServerListener listener { get; set; }

        NetEndPoint listeningOnEndPoint { get; }

        void Start(NetEndPoint endPoint);
        void Stop();

        void Update();
    }

    public class NetworkServer : INetworkServer, ITcpServerListener<TcpSocket>, IChannelListener {
        private readonly Object lockToken = new Object();
        private readonly TcpSocket tcpSocket;
        private readonly UdpSocket udpSocket;

        private readonly PlayerCollection<TcpSocket, ReliableChannel> socketCollection;

        private bool isAccepting = false;

        public ReliableChannel reliableChannel { get; }
        public UnreliableChannel unreliableChannel { get; }

        public NetEndPoint listeningOnEndPoint { get; private set; }

        public INetworkServerListener listener { get; set; }

        public NetworkServer(TcpSocket tcpSocket, UdpSocket udpSocket) {
            this.tcpSocket = tcpSocket;
            this.udpSocket = udpSocket;

            this.tcpSocket.serverListener = this;

            this.socketCollection = new PlayerCollection<TcpSocket, ReliableChannel>();

            this.reliableChannel = new ReliableChannel(this.tcpSocket);
            this.unreliableChannel = new UnreliableChannel(this.udpSocket) { listener = this, isServer = true };
        }

        public void Start(NetEndPoint endPoint) {
            this.tcpSocket.Bind(endPoint);
            this.tcpSocket.Start();

            this.udpSocket.Bind(endPoint);

            this.listeningOnEndPoint = this.tcpSocket.localEndPoint;

            this.unreliableChannel.StartIO(true, false, 4);
            // starts only input reading for server UDP socket
        }

        public void Stop() {
            this.tcpSocket.Stop();
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

        void ITcpServerListener<TcpSocket>.SocketDidAccept(TcpSocket socket) {
            if (socket == null) { return; }

            socket.serverListener = this;

            var reliable = new ReliableChannel(socket);
            var unreliable = new UnreliableChannel(new UdpSocket(udpSocket, socket.remoteEndPoint));

            reliable.StartIO();
            unreliable.StartIO(false, true, 1);
            // starts only output writing for client UDP fake socket

            this.socketCollection.Add(socket, reliable);
            this.listener?.NetworkServerDidAcceptPlayer(reliable, unreliable);

            lock (this.lockToken) { this.isAccepting = false; }
        }

        void ITcpServerListener<TcpSocket>.SocketDidDisconnect(TcpSocket socket) {
            var channel = this.socketCollection.Remove(socket);
            this.unreliableChannel.Unregister(socket.remoteEndPoint);
            if (channel == null) { return; }
            this.listener?.NetworkServerPlayerDidDisconnect(channel);
        }

        void IChannelListener.ChannelDidReceiveMessage(MessageContainer container, NetEndPoint from) {
            this.listener?.NetworkServerDidReceiveUnidentifiedMessage(container, from);
        }
    }
}