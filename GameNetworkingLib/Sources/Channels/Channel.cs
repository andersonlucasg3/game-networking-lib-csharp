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

    public abstract class Channel<TSocket> : IChannel, ISocketListener
        where TSocket : ISocket {
        private readonly TSocket socket;

        private readonly MessageStreamReader reader;
        private readonly MessageStreamWriter writer;

        public IChannelListener listener { get; set; }

        public Channel(TSocket socket) {
            this.socket = socket;
            this.socket.listener = this;

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
            while ((container = this.reader.Decode()) != null)
                { this.listener?.ChannelDidReceiveMessage(container); }
        }

        void ISocketListener.SocketDidWriteBytes(int count) => this.writer.DidWrite(count);
    }

    #region Reliable

    public class ReliableChannel : Channel<TcpSocket> {
        public ReliableChannel(TcpSocket socket) : base(socket) { }
    }

    #endregion

    #region Unreliable

    public class UnreliableChannel: Channel<UdpSocket> {
        public UnreliableChannel(UdpSocket socket) : base(socket) { }
    }

    #endregion
}