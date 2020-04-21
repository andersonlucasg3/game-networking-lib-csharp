using System.Collections.Concurrent;
using GameNetworking.Channels;
using GameNetworking.Sockets;

namespace GameNetworking.Networking {
    public interface INetworkServerListener {
        void NetworkServerDidAcceptPlayer(ReliableChannel channel);
        void NetworkServerPlayerDidDisconnect(ReliableChannel channel);
    }

    public interface INetworkServer {
        ReliableChannel reliableChannel { get; }
        UnreliableChannel unreliableChannel { get; }

        INetworkServerListener listener { get; set; }

        void Start(NetEndPoint endPoint);
        void Stop();

        void Update();
    }

    public class NetworkServer : INetworkServer, ITcpServerListener {
        private readonly ITcpSocket tcpSocket;
        private readonly ISocket udpSocket;

        private readonly ConcurrentDictionary<ITcpSocket, ReliableChannel> socketCollection;

        private bool isAccepting = false;

        public ReliableChannel reliableChannel { get; }
        public UnreliableChannel unreliableChannel { get; }

        public INetworkServerListener listener { get; set; }

        public NetworkServer(ITcpSocket tcpSocket, ISocket udpSocket) {
            this.tcpSocket = tcpSocket;
            this.udpSocket = udpSocket;

            this.socketCollection = new ConcurrentDictionary<ITcpSocket, ReliableChannel>();

            this.reliableChannel = new ReliableChannel(this.tcpSocket);
            this.unreliableChannel = new UnreliableChannel(this.udpSocket);
        }

        public void Start(NetEndPoint endPoint) {
            this.tcpSocket.Bind(endPoint);
            this.tcpSocket.Start();

            this.udpSocket.Bind(endPoint);
        }

        public void Update() {
            this.Accept();

            this.unreliableChannel.Receive();
            this.unreliableChannel.Flush();
        }

        public void Stop() {
            this.tcpSocket.Stop();
        }

        public void Close() {
            this.tcpSocket.Close();
            this.udpSocket.Close();
        }

        public void Accept() {
            lock(this) {
                if (this.isAccepting) { return; }
                this.isAccepting = true;
            }

            this.tcpSocket.Accept();
        }

        void ITcpServerListener.SocketDidAccept(ITcpSocket socket) {
            socket.serverListener = this;
            var channel = new ReliableChannel(socket);
            this.socketCollection[socket] = channel;
            this.listener?.NetworkServerDidAcceptPlayer(channel);

            lock(this) { this.isAccepting = false; }
        }

        void ITcpServerListener.SocketDidDisconnect(ITcpSocket socket) {
            if (this.socketCollection.TryRemove(socket, out ReliableChannel channel)) {
                this.listener?.NetworkServerPlayerDidDisconnect(channel);
            }
        }
    }
}