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

        void Start(NetEndPoint endPoint);
        void Stop();

        void Update();
    }

    public class NetworkServer : INetworkServer, ITcpServerListener<TcpSocket>, IUnreliableChannelListener {
        private readonly TcpSocket tcpSocket;
        private readonly UdpSocket udpSocket;

        private readonly PlayerCollection<TcpSocket, ReliableChannel> socketCollection;
        private readonly ConcurrentQueue<TcpSocket> socketsToRemove = new ConcurrentQueue<TcpSocket>();

        private bool isAccepting = false;

        public ReliableChannel reliableChannel { get; }
        public UnreliableChannel unreliableChannel { get; }

        public INetworkServerListener listener { get; set; }

        public NetworkServer(TcpSocket tcpSocket, UdpSocket udpSocket) {
            this.tcpSocket = tcpSocket;
            this.udpSocket = udpSocket;

            this.tcpSocket.serverListener = this;

            this.socketCollection = new PlayerCollection<TcpSocket, ReliableChannel>();

            this.reliableChannel = new ReliableChannel(this.tcpSocket);
            this.unreliableChannel = new UnreliableChannel(this.udpSocket) { serverListener = this };
        }

        public void Start(NetEndPoint endPoint) {
            this.tcpSocket.Bind(endPoint);
            this.tcpSocket.Start();

            this.udpSocket.Bind(endPoint);
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

            var values = this.socketCollection.values;
            for (int index = 0; index < values.Count; index++) {
                values[index].Flush();
                this.unreliableChannel.Flush();
            }
            this.RemoveSockets();
        }

        public void Accept() {
            lock (this) {
                if (this.isAccepting) { return; }
                this.isAccepting = true;
            }

            this.tcpSocket.Accept();
        }

        private void RemoveSockets() {
            while (this.socketsToRemove.TryDequeue(out TcpSocket socket)) {
                ReliableChannel channel = this.socketCollection.Remove(socket);
                if (channel == null) { return; }
                this.unreliableChannel.Unregister(socket.remoteEndPoint);
                this.listener?.NetworkServerPlayerDidDisconnect(channel);
            }
        }

        void ITcpServerListener<TcpSocket>.SocketDidAccept(TcpSocket socket) {
            if (socket == null) { return; }

            socket.serverListener = this;

            var reliable = new ReliableChannel(socket);
            var unreliable = new UnreliableChannel(new UdpSocket(udpSocket, socket.remoteEndPoint));

            this.socketCollection.Add(socket, reliable);
            this.listener?.NetworkServerDidAcceptPlayer(reliable, unreliable);

            lock (this) { this.isAccepting = false; }
        }

        void ITcpServerListener<TcpSocket>.SocketDidDisconnect(TcpSocket socket) {
            this.socketsToRemove.Enqueue(socket);
        }

        void IUnreliableChannelListener.UnreliableChannelDidReceiveMessage(MessageContainer container, NetEndPoint from) {
            this.listener?.NetworkServerDidReceiveUnidentifiedMessage(container, from);
        }
    }
}