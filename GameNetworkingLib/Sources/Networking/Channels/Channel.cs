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
        void Flush();
    }

    public abstract class Channel<TSocket> : IChannel, ISocketListener
        where TSocket : ISocket {
        private readonly MessageStreamReader reader;
        private readonly MessageStreamWriter writer;
        private bool isSending = false;
        private bool isReceiving = false;

        protected readonly TSocket socket;

        public IChannelListener listener { get; set; }

        public Channel(TSocket socket) {
            this.socket = socket;
            this.socket.listener = this;

            this.reader = new MessageStreamReader();
            this.writer = new MessageStreamWriter();
        }

        public void Receive() {
            lock(this.reader) {
                if (this.isReceiving) { return; }
                this.isReceiving = true;
            }

            this.socket.Receive();
        }

        public void Send(ITypedMessage message) {
            lock (this.writer) { this.writer.Write(message); }
            this.Flush();
        }

        public void Flush() {
            lock(this.writer) {
                if (this.isSending || !this.writer.hasBytesToWrite) { return; }
                this.isSending = true;

                var count = this.writer.Put(out byte[] buffer);
                this.socket.Send(buffer, count);
            }
        }

        void ISocketListener.SocketDidReceiveBytes(byte[] bytes, int count) {
            this.reader.Add(bytes, count);

            MessageContainer container;
            while ((container = this.reader.Decode()) != null)
                { this.listener?.ChannelDidReceiveMessage(container); }

            lock(this.reader) { this.isReceiving = false; }
        }

        void ISocketListener.SocketDidSendBytes(int count) {
            this.writer.DidWrite(count);

            lock(this.writer) { this.isSending = false; }
        }
    }

    public enum Channel {
        reliable, unreliable
    }

    #region Reliable

    public class ReliableChannel : Channel<ITcpSocket> {
        public ReliableChannel(ITcpSocket socket) : base(socket) { }

        public void CloseChannel() => this.socket.Disconnect();
    }

    #endregion

    #region Unreliable

    public class UnreliableChannel: Channel<ISocket> {
        public UnreliableChannel(ISocket socket) : base(socket) { }
    }

    #endregion
}