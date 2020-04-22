using System.Collections.Generic;
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

    public abstract class Channel<TSocket> : IChannel, ISocketListener<TSocket>
        where TSocket : ISocket<TSocket> {
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

        public virtual void Receive() {
            lock (this.reader) {
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
            lock (this.writer) {
                if (this.isSending || !this.writer.hasBytesToWrite) { return; }
                this.isSending = true;

                var count = this.writer.Put(out byte[] buffer);
                this.socket.Send(buffer, count);
            }
        }

        protected virtual void ChannelDidReceiveMessage(MessageContainer container, TSocket from) {
            this.listener?.ChannelDidReceiveMessage(container);
        }

        void ISocketListener<TSocket>.SocketDidReceiveBytes(TSocket socket, byte[] bytes, int count) {
            this.reader.Add(bytes, count);

            MessageContainer container;
            while ((container = this.reader.Decode()) != null) { this.ChannelDidReceiveMessage(container, socket); }

            lock (this.reader) { this.isReceiving = false; }
        }

        void ISocketListener<TSocket>.SocketDidSendBytes(TSocket socket, int count) {
            this.writer.DidWrite(count);

            lock (this.writer) { this.isSending = false; }
        }
    }

    public enum Channel {
        reliable, unreliable
    }

    #region Reliable

    public class ReliableChannel : Channel<TcpSocket> {
        public ReliableChannel(TcpSocket socket) : base(socket) { }

        public void CloseChannel() => this.socket.Disconnect();
    }

    #endregion

    #region Unreliable

    public interface IUnreliableChannelIdentifiedReceiveListener {
        void ChannelDidReceiveMessage(MessageContainer container);
    }

    public class UnreliableChannel : Channel<UdpSocket>, IUnreliableChannelIdentifiedReceiveListener {
        private readonly Dictionary<NetEndPoint, IUnreliableChannelIdentifiedReceiveListener> receiverCollection
            = new Dictionary<NetEndPoint, IUnreliableChannelIdentifiedReceiveListener>();
        private bool isServer = false;

        public UnreliableChannel(UdpSocket socket) : base(socket) { }

        public void Register(NetEndPoint endPoint, IUnreliableChannelIdentifiedReceiveListener listener) {
            this.isServer = true;
            this.receiverCollection[endPoint] = listener;
        }

        public void Unregister(NetEndPoint endPoint) => this.receiverCollection.Remove(endPoint);

        protected override void ChannelDidReceiveMessage(MessageContainer container, UdpSocket from) {
            if (!this.isServer) {
                this.listener?.ChannelDidReceiveMessage(container);
                return;
            }

            if (this.receiverCollection.TryGetValue(from.remoteEndPoint, out IUnreliableChannelIdentifiedReceiveListener listener)) {
                listener?.ChannelDidReceiveMessage(container);
            }
        }

        void IUnreliableChannelIdentifiedReceiveListener.ChannelDidReceiveMessage(MessageContainer container) {
            this.listener?.ChannelDidReceiveMessage(container);
        }
    }

    #endregion
}