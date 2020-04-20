using GameNetworking.Messages.Models;
using GameNetworking.Messages.Streams;
using GameNetworking.Sockets;

namespace GameNetworking.Channels {
    public interface IChannelListener {
        void ChannelDidReceiveMessage(MessageContainer container);
    }

    public interface IChannel {
        IChannelListener listener { get; set; }

        void Receive();
        void Send(ITypedMessage message);
    }

    public abstract class Channel<TSocket, TSocketListener> : IChannel, ISocketListener
        where TSocket : ISocket<TSocketListener>
        where TSocketListener : ISocketListener {
        private readonly TSocket socket;

        private readonly MessageStreamReader reader;
        private readonly MessageStreamWriter writer;

        public IChannelListener listener { get; set; }

        public Channel(TSocket socket) {
            this.socket = socket;

            this.reader = new MessageStreamReader();
            this.writer = new MessageStreamWriter();
        }

        public void Receive() => this.socket.Receive();

        public void Send(ITypedMessage message) {
            var count = this.writer.Write(message, out byte[] buffer);
            this.socket.Send(buffer, count);
        }

        void ISocketListener.SocketDidReadBytes(byte[] bytes, int count) {
            this.reader.Add(bytes, count);
            MessageContainer container;
            while ((container = this.reader.Decode()) != null) {
                this.listener?.ChannelDidReceiveMessage(container);
            }
        }

        void ISocketListener.SocketDidWriteBytes(int count) => this.writer.DidWrite(count);
    }

    public class ReliableChannel : Channel<TcpSocket, ITCPSocketListener>, ITCPSocketListener {
        public ReliableChannel(TcpSocket socket) : base(socket) {
            socket.listener = this;
        }

        void ITCPSocketListener.SocketDidAccept(TcpSocket socket) {
            
        }

        void ITCPSocketListener.SocketDidConnect() {
            throw new System.NotImplementedException();
        }

        void ITCPSocketListener.SocketDidDisconnect() {
            throw new System.NotImplementedException();
        }

        void ITCPSocketListener.SocketDidTimeout() {
            throw new System.NotImplementedException();
        }
    }
}