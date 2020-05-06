using GameNetworking.Messages.Models;
using GameNetworking.Messages.Streams;
using GameNetworking.Networking.Sockets;
using System.Threading;

namespace GameNetworking.Channels {
    public interface IReliableChannelListener {
        void ChannelDidReceiveMessage(ReliableChannel channel, MessageContainer container);
    }

    public class ReliableChannel : ITcpSocketIOListener<TcpSocket> {
        private readonly MessageStreamReader reader;
        private readonly MessageStreamWriter writer;
        private readonly TcpSocket socket;

        public IReliableChannelListener listener { get; set; }

        public ReliableChannel(TcpSocket socket) {
            this.socket = socket;
            this.socket.ioListener = this;

            this.reader = new MessageStreamReader();
            this.writer = new MessageStreamWriter();
        }

        public void CloseChannel() => this.socket.Disconnect();

        public void Send(ITypedMessage message) {
            this.writer.Write(message);
        }

        internal void StartIO() {
            ThreadPool.QueueUserWorkItem(_ => {
                do {
                    this.socket.Receive();
                    this.writer.Use(this.socket.Send);
                } while (this.socket.isConnected);
            });
        }

        void ITcpSocketIOListener<TcpSocket>.SocketDidReceiveBytes(TcpSocket socket, byte[] bytes, int count) {
            this.reader.Add(bytes, count);

            MessageContainer container = this.reader.Decode();
            if (container != null) {
                this.listener?.ChannelDidReceiveMessage(this, container);
            }
        }

        void ITcpSocketIOListener<TcpSocket>.SocketDidSendBytes(TcpSocket socket, int count) {
            this.writer.DidWrite(count);
        }
    }
}